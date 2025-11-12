# Scripts

This directory contains automation scripts to simplify common workflows with the azsdk CLI.

## Available Scripts

### generate-sdk-from-issue.sh

Automates TypeSpec SDK generation for all languages based on SDK generation requests from azure-rest-api-specs issues.

**Usage:**
```bash
./generate-sdk-from-issue.sh <typespec-project> <api-version> <release-type> <pr-number> [workitem-id]
```

**Example:**
```bash
# For issue #38691 requesting NetApp SDK generation
./generate-sdk-from-issue.sh \
  "specification/netapp/resource-manager/Microsoft.NetApp/NetApp" \
  "2025-09-01" \
  "stable" \
  38691
```

**What it does:**
1. Checks API readiness (validates PR approvals, TypeSpec project)
2. Initiates SDK generation pipelines for Python, .NET, JavaScript, Java, and Go
3. Reports build IDs and success/failure status for each language
4. Provides next steps for monitoring and linking PRs

**Parameters:**
- `typespec-project`: Path to the TypeSpec project in azure-rest-api-specs
- `api-version`: API version to generate (e.g., 2025-09-01)
- `release-type`: Either `beta` or `stable`
- `pr-number`: Pull request number in azure-rest-api-specs
- `workitem-id`: (Optional) Azure DevOps release plan work item ID

## Prerequisites

All scripts require:
- The `azsdk` CLI tool to be installed and in your PATH
- Proper authentication configured (az login, gh login)
- Appropriate Azure DevOps permissions

See the [main README](../README.md) for installation instructions.

## Related Documentation

- [TypeSpec SDK Generation Guide](../docs/typespec-sdk-generation-guide.md) - Comprehensive guide for TypeSpec SDK generation
- [azsdk CLI README](../Azure.Sdk.Tools.Cli/README.md) - Main CLI documentation
