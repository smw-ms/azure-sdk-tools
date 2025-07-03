using Azure.Sdk.Tools.Cli.Helpers;
using Azure.Sdk.Tools.Cli.Services;
using Azure.Sdk.Tools.Cli.Tools;
using Azure.Sdk.Tools.Cli.Models;
using Azure.Sdk.Tools.Cli.Models.Responses;
using Moq;

namespace Azure.Sdk.Tools.Cli.Tests.Tools
{
    [TestFixture]
    internal class SpecWorkflowToolTests
    {
        private Mock<IDevOpsService> mockDevOpsService;
        private Mock<IOutputService> mockOutputService;
        private SpecWorkflowTool specWorkflowTool;

        private Mock<ITypeSpecHelper> mockTypeSpecHelper;

        [SetUp]
        public void Setup()
        {
            mockDevOpsService = new Mock<IDevOpsService>();
            mockOutputService = new Mock<IOutputService>();
            mockTypeSpecHelper = new Mock<ITypeSpecHelper>();

            mockOutputService.Setup(x => x.Format(It.IsAny<GenericResponse>()))
                           .Returns((GenericResponse r) => string.Join(", ", r.Details));

            specWorkflowTool = new SpecWorkflowTool(
                new Mock<IGitHubService>().Object,
                mockDevOpsService.Object,
                new Mock<IGitHelper>().Object,
                new Mock<ITypeSpecHelper>().Object,
                mockOutputService.Object
            );
        }

        [Test]
        public async Task GenerateSDK_WhenPackageNameEmpty()
        {
            var releasePlan = new ReleasePlan
            {
                SDKInfo = new List<SDKInfo>
                {
                    new SDKInfo
                    {
                        Language = "python",
                        PackageName = ""
                    }
                }
            };

            mockDevOpsService.Setup(x => x.GetReleasePlanAsync(It.IsAny<int>()))
                           .ReturnsAsync(releasePlan);

            var result = await specWorkflowTool.GenerateSDK(
                typespecProjectRoot: "valid/path",
                apiVersion: "2023-01-01",
                sdkReleaseType: "beta",
                language: "python",
                pullRequestNumber: 123,
                workItemId: 456
            );

            Assert.That(result, Does.Contain("does not have a package name specified for python"));
        }

        [Test]
        public async Task GenerateSDK_WhenLanguageNotInReleasePlan()
        {
            // Test 1: Different language than requested
            var releasePlan = new ReleasePlan
            {
                SDKInfo = new List<SDKInfo>
                {
                    new SDKInfo
                    {
                        Language = "java", // Different language than requested
                        PackageName = "com.azure.test"
                    }
                }
            };

            mockDevOpsService.Setup(x => x.GetReleasePlanAsync(It.IsAny<int>()))
                           .ReturnsAsync(releasePlan);

            var result = await specWorkflowTool.GenerateSDK(
                typespecProjectRoot: "valid/path",
                apiVersion: "2023-01-01",
                sdkReleaseType: "beta",
                language: "python", // Requesting python but release plan has java
                pullRequestNumber: 123,
                workItemId: 456
            );

            Assert.That(result, Does.Contain("does not have a language specified"));

            // Test 2: Empty language
            var releasePlanWithEmptyLanguage = new ReleasePlan
            {
                SDKInfo = new List<SDKInfo>
                {
                    new SDKInfo
                    {
                        Language = "", // Empty language
                        PackageName = "some-package"
                    }
                }
            };

            mockDevOpsService.Setup(x => x.GetReleasePlanAsync(It.IsAny<int>()))
                           .ReturnsAsync(releasePlanWithEmptyLanguage);

            var resultEmptyLanguage = await specWorkflowTool.GenerateSDK(
                typespecProjectRoot: "valid/path",
                apiVersion: "2023-01-01",
                sdkReleaseType: "beta",
                language: "python",
                pullRequestNumber: 123,
                workItemId: 456
            );

            Assert.That(resultEmptyLanguage, Does.Contain("does not have a language specified"));
        }

        [Test]
        public async Task GenerateSDK_WhenSDKInfoListIsEmpty()
        {
            var releasePlan = new ReleasePlan
            {
                SDKInfo = new List<SDKInfo>() // Empty list - no SDK info at all
            };

            mockDevOpsService.Setup(x => x.GetReleasePlanAsync(It.IsAny<int>()))
                           .ReturnsAsync(releasePlan);

            var result = await specWorkflowTool.GenerateSDK(
                typespecProjectRoot: "valid/path",
                apiVersion: "2023-01-01",
                sdkReleaseType: "beta",
                language: "python",
                pullRequestNumber: 123,
                workItemId: 456
            );

            Assert.That(result, Does.Contain("SDK details are not present in the release plan"));
        }

        [Test]
        public async Task GenerateSDK_NamespaceApprovalRequired_NotApproved()
        {

        }

        [Test]
        public async Task GenerateSDK_NamespaceApprovalRequired_Approved()
        {

        }

        [Test]
        public async Task GenerateSDK_NamespaceApprovalNotRequired_DataPlane()
        {
            var releasePlan = new ReleasePlan
            {
                SDKInfo = new List<SDKInfo>
                {
                    new SDKInfo { Language = "python", PackageName = "azure-storage-test" }
                }
            };

            var package = new PackageResponse
            {
                Name = "azure-storage-test",
                Language = "python",
                ReleasedVersions = new List<SDKReleaseInfo>(), // First release
                PackageNameStatus = "Not approved" // Not approved, but not required for data plane
            };
    
            mockDevOpsService.Setup(x => x.GetPackageWorkItemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(package);
            mockTypeSpecHelper.Setup(x => x.IsTypeSpecProjectForMgmtPlane(It.IsAny<string>())).Returns(false);

            var result = await specWorkflowTool.GenerateSDK(
                typespecProjectRoot: "valid/path",
                apiVersion: "2023-01-01",
                sdkReleaseType: "beta",
                language: "python",
                pullRequestNumber: 123,
                workItemId: 456
            );
            
            Assert.That(result, Does.Contain("Namespace approval check completed"));
        }

        [Test]
        public async Task GenerateSDK_NamespaceApprovalNotRequired_NotFirstRelease()
        {

        }

        [Test]
        public async Task GenerateSDK_NamespaceApprovalNotRequired_StableRelease()
        {
            
        }
    }
}
