// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Framework.CommonTestUtils;
using Newtonsoft.Json.Linq;
using NuGet;
using Xunit;

namespace Microsoft.Framework.PackageManager
{
    public class DnuPackagesAddTests
    {
        private const string ProjectName = "HelloWorld";
        private static readonly SemanticVersion ProjectVersion = new SemanticVersion("0.1-beta");
        private const string Configuration = "Release";
        private const string PackagesDirName = "packages";
        private const string OutputDirName = "output";

        public static IEnumerable<object[]> RuntimeComponents
        {
            get
            {
                return TestUtils.GetRuntimeComponentsCombinations();
            }
        }

        [Theory]
        [MemberData("RuntimeComponents")]
        public void DnuPackagesAddSkipsInstalledPackageWhenShasMatch(string flavor, string os, string architecture)
        {
            var runtimeHomeDir = TestUtils.GetRuntimeHomeDir(flavor, os, architecture);
            using (var tempSamplesDir = TestUtils.PrepareTemporarySamplesFolder(runtimeHomeDir))
            {
                var projectFilePath = Path.Combine(tempSamplesDir, ProjectName, Runtime.Project.ProjectFileName);
                var packagesDir = Path.Combine(tempSamplesDir, PackagesDirName);
                var packagePathResolver = new DefaultPackagePathResolver(packagesDir);
                var nuspecPath = packagePathResolver.GetManifestFilePath(ProjectName, ProjectVersion);
                var hashFilePath = packagePathResolver.GetHashPath(ProjectName, ProjectVersion);

                BuildPackage(tempSamplesDir, runtimeHomeDir);

                string stdOut;
                var exitCode = DnuPackagesAddOutputPackage(tempSamplesDir, runtimeHomeDir, out stdOut);
                Assert.Equal(0, exitCode);
                Assert.Contains($"Installing {ProjectName}.{ProjectVersion}", stdOut);

                var lastInstallTime = new FileInfo(nuspecPath).LastWriteTimeUtc;
                var hashBeforeReAdding = File.ReadAllText(hashFilePath);

                exitCode = DnuPackagesAddOutputPackage(tempSamplesDir, runtimeHomeDir, out stdOut);
                Assert.Equal(0, exitCode);
                Assert.Contains($"{ProjectName}.{ProjectVersion} already exists and won't be overwritten because it is identical", stdOut);
                Assert.Equal(lastInstallTime, new FileInfo(nuspecPath).LastWriteTimeUtc);
                Assert.Equal(hashBeforeReAdding, File.ReadAllText(hashFilePath));
            }
        }

        [Theory]
        [MemberData("RuntimeComponents")]
        public void DnuPackagesAddOverwritesInstalledPackageWhenShasDoNotMatch(string flavor, string os, string architecture)
        {
            var runtimeHomeDir = TestUtils.GetRuntimeHomeDir(flavor, os, architecture);
            using (var tempSamplesDir = TestUtils.PrepareTemporarySamplesFolder(runtimeHomeDir))
            {
                var projectFilePath = Path.Combine(tempSamplesDir, ProjectName, Runtime.Project.ProjectFileName);
                var packagesDir = Path.Combine(tempSamplesDir, PackagesDirName);
                var packagePathResolver = new DefaultPackagePathResolver(packagesDir);
                var nuspecPath = packagePathResolver.GetManifestFilePath(ProjectName, ProjectVersion);
                var hashFilePath = packagePathResolver.GetHashPath(ProjectName, ProjectVersion);
                var outputPackagePath = Path.Combine(tempSamplesDir, OutputDirName, Configuration,
                    $"{ProjectName}.{ProjectVersion}{NuGet.Constants.PackageExtension}");

                SetProjectDescription(projectFilePath, "Old");
                BuildPackage(tempSamplesDir, runtimeHomeDir);

                string stdOut;
                var exitCode = DnuPackagesAddOutputPackage(tempSamplesDir, runtimeHomeDir, out stdOut);
                Assert.Equal(0, exitCode);
                Assert.Contains($"Installing {ProjectName}.{ProjectVersion}", stdOut);

                var lastInstallTime = new FileInfo(nuspecPath).LastWriteTimeUtc;
                var hashBeforeReAdding = File.ReadAllText(hashFilePath);

                SetProjectDescription(projectFilePath, "New");
                BuildPackage(tempSamplesDir, runtimeHomeDir);

                var newPackageHash = TestUtils.ComputeSHA(outputPackagePath);

                exitCode = DnuPackagesAddOutputPackage(tempSamplesDir, runtimeHomeDir, out stdOut);
                Assert.Equal(0, exitCode);
                Assert.Contains($"Overwriting {ProjectName}.{ProjectVersion}", stdOut);

                var xDoc = XDocument.Load(packagePathResolver.GetManifestFilePath(ProjectName, ProjectVersion));
                var actualDescription = xDoc.Root.Descendants()
                    .Single(x => string.Equals(x.Name.LocalName, "description")).Value;
                Assert.Equal("New", actualDescription);
                Assert.NotEqual(lastInstallTime, new FileInfo(nuspecPath).LastWriteTimeUtc);
                Assert.NotEqual(hashBeforeReAdding, File.ReadAllText(hashFilePath));
                Assert.Equal(newPackageHash, File.ReadAllText(hashFilePath));
            }
        }

        private static void SetProjectDescription(string projectFilePath, string description)
        {
            var json = JObject.Parse(File.ReadAllText(projectFilePath));
            json["description"] = description;
            File.WriteAllText(projectFilePath, json.ToString());
        }

        private static void BuildPackage(string sampleDir, string runtimeHomeDir)
        {
            var projectDir = Path.Combine(sampleDir, ProjectName);
            var buildOutpuDir = Path.Combine(sampleDir, OutputDirName);
            int exitCode = DnuTestUtils.ExecDnu(
                runtimeHomeDir,
                "pack",
                $"{projectDir} --out {buildOutpuDir} --configuration {Configuration}",
                environment: new Dictionary<string, string> { { "DNX_BUILD_VERSION", null } });
            Assert.Equal(0, exitCode);
        }

        private static int DnuPackagesAddOutputPackage(string sampleDir, string runtimeHomeDir, out string stdOut)
        {
            var packagePath = Path.Combine(sampleDir, OutputDirName, Configuration,
                $"{ProjectName}.{ProjectVersion}{NuGet.Constants.PackageExtension}");
            var packagesDir = Path.Combine(sampleDir, PackagesDirName);

            string stdErr;
            var exitCode = DnuTestUtils.ExecDnu(
                runtimeHomeDir,
                "packages",
                $"add {packagePath} {packagesDir}",
                out stdOut,
                out stdErr);

            return exitCode;
        }
    }
}
