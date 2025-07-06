using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using static UnityEditor.PackageManager.UI.Sample;

/// <summary>
/// This script is indtended to install samples which are required for some features of this package.
/// The list of samples to install is described in required_samples.json.
/// </summary>
namespace Styly.XRRig
{
    public static class InstallRequiredSamples
    {
        // Required samples information JSON file name
        static readonly string RequiredSamplesJson = "required_samples.json";

        /// <summary>
        /// Install package samples described in required_samples.json
        /// </summary>
        [InitializeOnLoadMethod]
        public static void InstallPackageSamples()
        {
            // Get the path of this package
            var MyPackageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.Assembly);
            string MyPackagePath = MyPackageInfo.resolvedPath;

            // Read the JSON file that contains the list of samples to install
            string RequiredSamplesJsonPath = Path.Combine(MyPackagePath, RequiredSamplesJson);
            string jsonData = File.ReadAllText(RequiredSamplesJsonPath);
            SamplePackageInfo packageInfo = JsonUtility.FromJson<SamplePackageInfo>(jsonData);
            foreach (var sampleToInstall in packageInfo.samples)
            {   
                var packageName = sampleToInstall.PackageName;
                var SampleName = sampleToInstall.SampleName;
                InstallSample(packageName, SampleName);
            }
        }

        /// <summary>
        /// Install a specific sample from a package
        /// </summary>
        /// <param name="packageName">Package name containing the sample</param>
        /// <param name="sampleName">Sample name to install</param>
        public static void InstallSample(string packageName, string sampleName)
        {
            var packageInfomation = GetPackageInfo(packageName);
            var Samples = UnityEditor.PackageManager.UI.Sample.FindByPackage(packageName, packageInfomation.version);
            foreach (var sample in Samples)
            {
                if (sample.displayName == sampleName)
                {
                    // Install the sample if it is not imported
                    if (!sample.isImported)
                    {
                        sample.Import(ImportOptions.OverridePreviousImports);
                        Debug.Log("Installed sample: " + sample.displayName + " (" + packageInfomation.displayName + " @ " + packageInfomation.version + ")");
                    }
                }
            }
        }

        [Serializable]
        private class SamplePackageInfo
        {
            public Sample[] samples;
        }

        [Serializable]
        private class Sample
        {
            public string PackageName;
            public string SampleName;
        }

        /// <summary>
        /// Get package information by package name
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static UnityEditor.PackageManager.PackageInfo GetPackageInfo(string packageName)
        {
            var request = Client.List(true, true);
            while (!request.IsCompleted) { }
            if (request.Status == StatusCode.Success) { return request.Result.FirstOrDefault(pkg => pkg.name == packageName); }
            return null;
        }



    }
}