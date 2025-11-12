# Example: NetApp SDK Generation (Issue #38691)

This example demonstrates how to use the azsdk CLI to process SDK generation request from issue [#38691](https://github.com/Azure/azure-rest-api-specs/issues/38691).

## Issue Details

- **Service**: NetApp
- **TypeSpec Project**: `specification/netapp/resource-manager/Microsoft.NetApp/NetApp`
- **API Version**: `2025-09-01`
- **Release Type**: `Stable`
- **Languages**: Java, JavaScript, Python, .NET, Go
- **Spec Type**: TypeSpec (Management Plane)

## Prerequisites

Before starting, ensure:
1. The azsdk CLI is installed and configured
2. You have authenticated with Azure DevOps (for pipeline access)
3. The TypeSpec pull request has the required approvals:
   - ARMSignedOff label
   - APIStewardshipBoard-SignedOff label

## Step-by-Step Process

### Option 1: Using the Automation Script

The easiest way to generate SDKs for all languages:

```bash
cd tools/azsdk-cli/scripts
./generate-sdk-from-issue.sh \
  "specification/netapp/resource-manager/Microsoft.NetApp/NetApp" \
  "2025-09-01" \
  "stable" \
  38691
```

This will:
1. Check API readiness
2. Generate SDKs for all 5 languages
3. Provide build IDs for tracking
4. Report success/failure for each language

### Option 2: Manual Step-by-Step

#### Step 1: Check API Readiness

```bash
azsdk spec-workflow check-api-readiness \
  --typespec-project "specification/netapp/resource-manager/Microsoft.NetApp/NetApp" \
  --pr 38691
```

Expected output:
```
✓ TypeSpec project path is valid
✓ Pull request #38691 exists and is properly configured
✓ Pull request has required approvals (ARMSignedOff, APIStewardshipBoard-SignedOff)
✓ Pull request targets main branch
Status: Success - API spec is ready for SDK generation
```

#### Step 2: Generate SDKs for Each Language

**Python:**
```bash
azsdk spec-workflow generate-sdk \
  --typespec-project "specification/netapp/resource-manager/Microsoft.NetApp/NetApp" \
  --api-version "2025-09-01" \
  --release-type "stable" \
  --language "python" \
  --pr 38691
```

**.NET:**
```bash
azsdk spec-workflow generate-sdk \
  --typespec-project "specification/netapp/resource-manager/Microsoft.NetApp/NetApp" \
  --api-version "2025-09-01" \
  --release-type "stable" \
  --language ".net" \
  --pr 38691
```

**JavaScript:**
```bash
azsdk spec-workflow generate-sdk \
  --typespec-project "specification/netapp/resource-manager/Microsoft.NetApp/NetApp" \
  --api-version "2025-09-01" \
  --release-type "stable" \
  --language "javascript" \
  --pr 38691
```

**Java:**
```bash
azsdk spec-workflow generate-sdk \
  --typespec-project "specification/netapp/resource-manager/Microsoft.NetApp/NetApp" \
  --api-version "2025-09-01" \
  --release-type "stable" \
  --language "java" \
  --pr 38691
```

**Go:**
```bash
azsdk spec-workflow generate-sdk \
  --typespec-project "specification/netapp/resource-manager/Microsoft.NetApp/NetApp" \
  --api-version "2025-09-01" \
  --release-type "stable" \
  --language "go" \
  --pr 38691
```

Each command will return:
```
Status: Success
Details: Azure DevOps pipeline <pipeline-url> has been initiated to generate the SDK. Build ID is <build-id>. Once the pipeline job completes, an SDK pull request for <language> will be created.
```

#### Step 3: Monitor Pipeline Progress

Track the pipeline runs in Azure DevOps using the provided URLs or build IDs.

#### Step 4: Retrieve SDK Pull Request Links

Once pipelines complete, get the PR links:

```bash
azsdk spec-workflow get-sdk-pr \
  --language "python" \
  --pipeline-run <build-id-from-step-2>
```

Repeat for each language.

#### Step 5: Link PRs to Release Plan (Optional)

If you have a release plan work item:

```bash
azsdk spec-workflow link-sdk-pr \
  --language "python" \
  --url "https://github.com/Azure/azure-sdk-for-python/pull/<pr-number>" \
  --workitem-id <workitem-id>
```

## Expected Results

After successful completion, you should have:
- 5 SDK pull requests (one per language):
  - azure-sdk-for-python
  - azure-sdk-for-net
  - azure-sdk-for-js
  - azure-sdk-for-java
  - azure-sdk-for-go
- All PRs linked to the release plan (if workitem-id provided)
- SDKs ready for review and release

## Troubleshooting

### API Readiness Check Fails

**Problem**: Missing required approvals

**Solution**: Ensure the PR has both:
- `ARMSignedOff` label
- `APIStewardshipBoard-SignedOff` label

**Problem**: PR not targeting main branch

**Solution**: The PR must target the `main` branch. Create a new PR if needed.

### SDK Generation Fails

**Problem**: Invalid TypeSpec project path

**Solution**: Verify the path exists in azure-rest-api-specs and contains a `tspconfig.yaml` file:
```bash
ls specification/netapp/resource-manager/Microsoft.NetApp/NetApp/tspconfig.yaml
```

**Problem**: Pipeline permissions

**Solution**: Ensure you have the necessary Azure DevOps permissions to trigger SDK generation pipelines.

## Additional Notes

- SDK generation is triggered through Azure DevOps pipelines
- The actual SDK generation happens asynchronously
- Pipeline runs can take 30-60 minutes depending on the language
- SDKs should not be released immediately (per issue requirements)
- Release will be handled later by the service owner

## References

- [Issue #38691](https://github.com/Azure/azure-rest-api-specs/issues/38691)
- [TypeSpec SDK Generation Guide](../typespec-sdk-generation-guide.md)
- [azsdk CLI Documentation](../../README.md)
