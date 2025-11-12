#!/bin/bash
# generate-sdk-from-issue.sh
#
# Automates SDK generation for TypeSpec projects based on azure-rest-api-specs issue requests
#
# Usage: ./generate-sdk-from-issue.sh <typespec-project> <api-version> <release-type> <pr-number> [workitem-id]
# Example: ./generate-sdk-from-issue.sh 'specification/netapp/resource-manager/Microsoft.NetApp/NetApp' '2025-09-01' 'stable' 38691

set -e

TYPESPEC_PROJECT="$1"
API_VERSION="$2"
RELEASE_TYPE="$3"
PR_NUMBER="$4"
WORKITEM_ID="${5:-}"

# Validate inputs
if [ -z "$TYPESPEC_PROJECT" ] || [ -z "$API_VERSION" ] || [ -z "$RELEASE_TYPE" ] || [ -z "$PR_NUMBER" ]; then
  echo "Error: Missing required arguments"
  echo ""
  echo "Usage: $0 <typespec-project> <api-version> <release-type> <pr-number> [workitem-id]"
  echo ""
  echo "Arguments:"
  echo "  typespec-project   Path to TypeSpec project in azure-rest-api-specs"
  echo "  api-version        API version to generate (e.g., 2025-09-01)"
  echo "  release-type       SDK release type: 'beta' or 'stable'"
  echo "  pr-number          Pull request number in azure-rest-api-specs"
  echo "  workitem-id        (Optional) Azure DevOps release plan work item ID"
  echo ""
  echo "Example:"
  echo "  $0 'specification/netapp/resource-manager/Microsoft.NetApp/NetApp' '2025-09-01' 'stable' 38691"
  exit 1
fi

# Validate release type
if [ "$RELEASE_TYPE" != "beta" ] && [ "$RELEASE_TYPE" != "stable" ]; then
  echo "Error: release-type must be 'beta' or 'stable'"
  exit 1
fi

# Languages to generate SDKs for
LANGUAGES=("python" ".net" "javascript" "java" "go")

echo "========================================="
echo "TypeSpec SDK Generation"
echo "========================================="
echo "TypeSpec Project: $TYPESPEC_PROJECT"
echo "API Version:      $API_VERSION"
echo "Release Type:     $RELEASE_TYPE"
echo "PR Number:        $PR_NUMBER"
if [ -n "$WORKITEM_ID" ]; then
  echo "Work Item ID:     $WORKITEM_ID"
fi
echo "Languages:        ${LANGUAGES[*]}"
echo "========================================="
echo ""

# Build workitem argument if provided
WORKITEM_ARG=""
if [ -n "$WORKITEM_ID" ]; then
  WORKITEM_ARG="--workitem-id $WORKITEM_ID"
fi

# Step 1: Check API readiness
echo "Step 1: Checking API readiness..."
if azsdk spec-workflow check-api-readiness \
  --typespec-project "$TYPESPEC_PROJECT" \
  --pr "$PR_NUMBER" \
  $WORKITEM_ARG; then
  echo "✓ API readiness check passed"
else
  echo "✗ API readiness check failed"
  echo ""
  echo "Please resolve the issues reported above before generating SDKs."
  echo "Common issues include:"
  echo "  - Missing required approvals (ARMSignedOff, APIStewardshipBoard-SignedOff)"
  echo "  - Pull request not targeting main branch"
  echo "  - Invalid TypeSpec project path"
  exit 1
fi

echo ""
echo "Step 2: Generating SDKs for all languages..."
echo ""

# Arrays to track results
declare -A BUILD_IDS
declare -A RESULTS

# Generate SDK for each language
for lang in "${LANGUAGES[@]}"; do
  echo "Generating SDK for $lang..."
  
  # Run the generation command and capture output
  if OUTPUT=$(azsdk spec-workflow generate-sdk \
    --typespec-project "$TYPESPEC_PROJECT" \
    --api-version "$API_VERSION" \
    --release-type "$RELEASE_TYPE" \
    --language "$lang" \
    --pr "$PR_NUMBER" \
    $WORKITEM_ARG 2>&1); then
    
    echo "✓ SDK generation initiated for $lang"
    RESULTS[$lang]="success"
    
    # Try to extract build ID from output
    BUILD_ID=$(echo "$OUTPUT" | grep -oP 'Build ID is \K\d+' || echo "")
    if [ -n "$BUILD_ID" ]; then
      BUILD_IDS[$lang]=$BUILD_ID
      echo "  Build ID: $BUILD_ID"
    fi
  else
    echo "✗ SDK generation failed for $lang"
    echo "  Error: $OUTPUT"
    RESULTS[$lang]="failed"
  fi
  echo ""
done

# Step 3: Summary
echo "========================================="
echo "SDK Generation Summary"
echo "========================================="

SUCCESS_COUNT=0
FAILED_COUNT=0

for lang in "${LANGUAGES[@]}"; do
  STATUS="${RESULTS[$lang]}"
  if [ "$STATUS" = "success" ]; then
    SUCCESS_COUNT=$((SUCCESS_COUNT + 1))
    if [ -n "${BUILD_IDS[$lang]}" ]; then
      echo "✓ $lang: Pipeline initiated (Build ID: ${BUILD_IDS[$lang]})"
    else
      echo "✓ $lang: Pipeline initiated"
    fi
  else
    FAILED_COUNT=$((FAILED_COUNT + 1))
    echo "✗ $lang: Failed"
  fi
done

echo ""
echo "Results: $SUCCESS_COUNT succeeded, $FAILED_COUNT failed out of ${#LANGUAGES[@]} languages"
echo ""

if [ $SUCCESS_COUNT -gt 0 ]; then
  echo "Next steps:"
  echo "1. Monitor pipeline runs in Azure DevOps"
  echo "2. Once pipelines complete, retrieve PR links using:"
  echo "   azsdk spec-workflow get-sdk-pr --language <language> --pipeline-run <build-id>"
  if [ -n "$WORKITEM_ID" ]; then
    echo "3. Link SDK PRs to release plan:"
    echo "   azsdk spec-workflow link-sdk-pr --language <language> --url <pr-url> --workitem-id $WORKITEM_ID"
  fi
fi

echo ""
echo "========================================="

# Exit with error if any language failed
if [ $FAILED_COUNT -gt 0 ]; then
  exit 1
fi

exit 0
