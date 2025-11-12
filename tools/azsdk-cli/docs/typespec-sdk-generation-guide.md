# TypeSpec SDK Generation Guide

This guide explains how to use the `azsdk` CLI tool to generate SDKs from TypeSpec specifications, particularly in response to SDK generation requests from the azure-rest-api-specs repository.

## Overview

The `azsdk spec-workflow` command provides tools to automate TypeSpec-based SDK generation across multiple languages. This is typically used when:
- A new Azure service API is defined using TypeSpec
- An existing service API has been updated
- SDK generation is requested via issues in the azure-rest-api-specs repository

## Prerequisites

Before generating SDKs, ensure you have:
1. The azsdk CLI tool installed (see main [README](../README.md))
2. Access to the azure-rest-api-specs repository
3. Appropriate Azure DevOps permissions for SDK generation pipelines
4. Authentication configured (az login, gh login)

## Available Commands

The `spec-workflow` group includes these commands:
- `check-api-readiness` - Verify that an API spec is ready for SDK generation
- `generate-sdk` - Trigger SDK generation for a TypeSpec project
- `get-sdk-pr` - Get SDK pull request details from a generation pipeline run
- `link-sdk-pr` - Link an SDK pull request to a release plan

## Common Workflow

### 1. Check API Readiness

Before generating SDKs, verify that the TypeSpec project is ready:

```bash
azsdk spec-workflow check-api-readiness \
  --typespec-project "specification/netapp/resource-manager/Microsoft.NetApp/NetApp" \
  --pr 38691 \
  --workitem-id <workitem-id>
```

This command checks:
- The TypeSpec project path is valid
- The pull request exists and is properly configured  
- Required approvals are in place (ARMSignedOff, APIStewardshipBoard-SignedOff)
- The pull request targets the main branch

### 2. Generate SDK

Once the API is ready, generate SDKs for each language:

```bash
# For Python
azsdk spec-workflow generate-sdk \
  --typespec-project "specification/netapp/resource-manager/Microsoft.NetApp/NetApp" \
  --api-version "2025-09-01" \
  --release-type "stable" \
  --language "python" \
  --pr 38691 \
  --workitem-id <workitem-id>

# For .NET
azsdk spec-workflow generate-sdk \
  --typespec-project "specification/netapp/resource-manager/Microsoft.NetApp/NetApp" \
  --api-version "2025-09-01" \
  --release-type "stable" \
  --language ".net" \
  --pr 38691 \
  --workitem-id <workitem-id>

# For JavaScript
azsdk spec-workflow generate-sdk \
  --typespec-project "specification/netapp/resource-manager/Microsoft.NetApp/NetApp" \
  --api-version "2025-09-01" \
  --release-type "stable" \
  --language "javascript" \
  --pr 38691 \
  --workitem-id <workitem-id>

# For Java
azsdk spec-workflow generate-sdk \
  --typespec-project "specification/netapp/resource-manager/Microsoft.NetApp/NetApp" \
  --api-version "2025-09-01" \
  --release-type "stable" \
  --language "java" \
  --pr 38691 \
  --workitem-id <workitem-id>

# For Go
azsdk spec-workflow generate-sdk \
  --typespec-project "specification/netapp/resource-manager/Microsoft.NetApp/NetApp" \
  --api-version "2025-09-01" \
  --release-type "stable" \
  --language "go" \
  --pr 38691 \
  --workitem-id <workitem-id>
```

Each command will:
- Validate the request parameters
- Trigger the SDK generation pipeline in Azure DevOps
- Return a build ID for tracking progress

### 3. Get SDK Pull Request Details

After the generation pipeline completes, retrieve the SDK pull request URLs:

```bash
azsdk spec-workflow get-sdk-pr \
  --language "python" \
  --pipeline-run <build-id> \
  --workitem-id <workitem-id>
```

### 4. Link SDK PR to Release Plan

Link the generated SDK pull requests to the release plan:

```bash
azsdk spec-workflow link-sdk-pr \
  --language "python" \
  --url "https://github.com/Azure/azure-sdk-for-python/pull/<pr-number>" \
  --workitem-id <workitem-id>
```

## Example: Processing Issue #38691

Issue [#38691](https://github.com/Azure/azure-rest-api-specs/issues/38691) requests SDK generation for NetApp with these parameters:
- **TypeSpec Project**: `specification/netapp/resource-manager/Microsoft.NetApp/NetApp`
- **API Version**: `2025-09-01`
- **Release Type**: `Stable`
- **Languages**: Java, JavaScript, Python, .NET, Go

### Step-by-Step Process

```bash
# 1. Check if the API spec is ready
azsdk spec-workflow check-api-readiness \
  --typespec-project "specification/netapp/resource-manager/Microsoft.NetApp/NetApp" \
  --pr 38691

# 2. Generate SDKs for all languages
for lang in python .net javascript java go; do
  echo "Generating SDK for $lang..."
  azsdk spec-workflow generate-sdk \
    --typespec-project "specification/netapp/resource-manager/Microsoft.NetApp/NetApp" \
    --api-version "2025-09-01" \
    --release-type "stable" \
    --language "$lang" \
    --pr 38691
done

# 3. After pipelines complete, get PR links and link to release plan
# (This step requires the build IDs from step 2)
```

## Automation Script

For convenience, you can use the following script to automate SDK generation requests:

```bash
#!/bin/bash
# generate-sdk-from-issue.sh
#
# Usage: ./generate-sdk-from-issue.sh <typespec-project> <api-version> <release-type> <pr-number> [workitem-id]

TYPESPEC_PROJECT="$1"
API_VERSION="$2"
RELEASE_TYPE="$3"
PR_NUMBER="$4"
WORKITEM_ID="${5:-}"

if [ -z "$TYPESPEC_PROJECT" ] || [ -z "$API_VERSION" ] || [ -z "$RELEASE_TYPE" ] || [ -z "$PR_NUMBER" ]; then
  echo "Usage: $0 <typespec-project> <api-version> <release-type> <pr-number> [workitem-id]"
  echo "Example: $0 'specification/netapp/resource-manager/Microsoft.NetApp/NetApp' '2025-09-01' 'stable' 38691"
  exit 1
fi

# Languages to generate
LANGUAGES=("python" ".net" "javascript" "java" "go")

echo "Checking API readiness..."
WORKITEM_ARG=""
if [ -n "$WORKITEM_ID" ]; then
  WORKITEM_ARG="--workitem-id $WORKITEM_ID"
fi

azsdk spec-workflow check-api-readiness \
  --typespec-project "$TYPESPEC_PROJECT" \
  --pr "$PR_NUMBER" \
  $WORKITEM_ARG

if [ $? -ne 0 ]; then
  echo "API readiness check failed. Please resolve issues before generating SDKs."
  exit 1
fi

echo "API is ready. Generating SDKs for all languages..."

# Generate SDK for each language
for lang in "${LANGUAGES[@]}"; do
  echo "Generating SDK for $lang..."
  azsdk spec-workflow generate-sdk \
    --typespec-project "$TYPESPEC_PROJECT" \
    --api-version "$API_VERSION" \
    --release-type "$RELEASE_TYPE" \
    --language "$lang" \
    --pr "$PR_NUMBER" \
    $WORKITEM_ARG
    
  if [ $? -eq 0 ]; then
    echo "✓ SDK generation initiated for $lang"
  else
    echo "✗ SDK generation failed for $lang"
  fi
done

echo "SDK generation pipeline runs initiated for all languages."
echo "Use 'azsdk spec-workflow get-sdk-pr' to check status and get PR links."
```

## Command Options

### generate-sdk

| Option | Required | Description |
|--------|----------|-------------|
| `--typespec-project` | Yes | Path to the TypeSpec project in azure-rest-api-specs |
| `--api-version` | Yes | API version to generate (e.g., 2025-09-01) |
| `--release-type` | Yes | SDK release type: `beta` or `stable` |
| `--language` | Yes | Target language: `python`, `.net`, `javascript`, `java`, or `go` |
| `--pr` | No | Pull request number in azure-rest-api-specs |
| `--workitem-id` | No | Azure DevOps release plan work item ID |

### check-api-readiness

| Option | Required | Description |
|--------|----------|-------------|
| `--typespec-project` | Yes | Path to the TypeSpec project |
| `--pr` | No | Pull request number to check |
| `--workitem-id` | No | Work item ID to update with approval status |

### get-sdk-pr

| Option | Required | Description |
|--------|----------|-------------|
| `--language` | Yes | SDK language to query |
| `--pipeline-run` | Yes* | Azure DevOps build/pipeline run ID |
| `--workitem-id` | Yes* | Release plan work item ID |

*Either `--pipeline-run` or `--workitem-id` is required

### link-sdk-pr

| Option | Required | Description |
|--------|----------|-------------|
| `--language` | Yes | SDK language |
| `--url` | Yes | SDK pull request URL |
| `--workitem-id` | No | Work item ID to link PR to |
| `--release-plan` | No | Release plan ID to link PR to |

*Either `--workitem-id` or `--release-plan` is required

## Tips

- Always run `check-api-readiness` before generating SDKs to catch issues early
- Keep track of build IDs returned by `generate-sdk` for later reference
- The `--workitem-id` parameter is optional but recommended for tracking releases
- SDK generation runs asynchronously in Azure DevOps pipelines
- Use JSON output format (`-o json`) for programmatic processing

## Troubleshooting

### API Readiness Checks Fail

If `check-api-readiness` fails, common issues include:
- Pull request doesn't have required approvals (ARMSignedOff, APIStewardshipBoard-SignedOff)
- Pull request is not targeting the main branch
- TypeSpec project path is incorrect
- Pull request is closed or merged

### SDK Generation Fails

Common causes:
- Invalid TypeSpec syntax in the project
- Missing dependencies in the TypeSpec configuration
- Incorrect API version format
- Azure DevOps pipeline permissions

## Further Reading

- [TypeSpec Project Scripts Documentation](../../../doc/common/TypeSpec-Project-Scripts.md)
- [azsdk CLI README](../README.md)
- [Azure SDK Release Process](https://aka.ms/azsdk/releases)
