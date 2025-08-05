using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Unity.XR.CoreUtils.Editor;

public static class XRProjectValidationFixAll
{
    public static void FixAllIssuesForAllPlatforms(string[] ignoredIssueStrings = null)
    {
        // Define all potential platforms
        var allPlatforms = new[]
        {
            BuildTargetGroup.Standalone,
            BuildTargetGroup.Android,
            BuildTargetGroup.iOS,
            BuildTargetGroup.VisionOS
        };

        // Filter to only include platforms with installed modules
        var availablePlatforms = allPlatforms.Where(IsPlatformModuleInstalled).ToArray();

        // Loop for FixAllIssues for each available platform
        foreach (var platform in availablePlatforms) { FixAllIssues(platform, ignoredIssueStrings); }
    }

    /// <summary>
    /// Fixes all project validation issues for the specified build target group.
    /// This method collects all issues across all platforms and applies fixes where possible.
    /// </summary>
    /// <param name="targetGroup">The build target group to fix issues for.</param>
    /// <param name="ignoredIssueStrings">Optional array of issue strings to ignore.</param>
    /// <remarks>
    /// This method collects all issues across all platforms and applies fixes where possible.
    /// </remarks>
    /// <example>
    /// <code>
    /// XRProjectValidationFixAll.FixAllIssues(BuildTargetGroup.Android, new[] {
    ///    "Some specific issue to ignore"
    /// });
    /// </code>
    /// </example>
    public static void FixAllIssues(BuildTargetGroup targetGroup, string[] ignoredIssueStrings = null)
    {
        var (allIssues, ignoredCount) = CollectIssuesForPlatforms(new[] { targetGroup }, ignoredIssueStrings);

        if (allIssues.Count == 0)
        {
            Debug.Log($"No fixable issues found for {targetGroup}.");
            return;
        }

        BuildValidator.FixIssues(allIssues.ToList());
        Debug.Log($"Project Validation: Fixed {allIssues.Count} issues for {targetGroup}. (Ignored {ignoredCount} issues)");
    }

    // Helper method to collect issues for specific platforms
    private static (HashSet<BuildValidationRule> issues, int ignoredCount) CollectIssuesForPlatforms(
        BuildTargetGroup[] targetGroups = null, string[] ignoredStrings = null)
    {
        Type buildValidatorType = typeof(BuildValidator);

        // internal static void GetCurrentValidationIssues(HashSet<BuildValidationRule>, BuildTargetGroup)
        MethodInfo getIssues =
            buildValidatorType.GetMethod(
                "GetCurrentValidationIssues",
                BindingFlags.NonPublic | BindingFlags.Static);

        if (getIssues == null)
        {
            Debug.LogError(
                "GetCurrentValidationIssues not found. Please check the version of XR Core Utilities.");
            return (new HashSet<BuildValidationRule>(), 0);
        }

        HashSet<BuildValidationRule> allIssues = new();
        int ignoredCount = 0;

        // If no specific groups provided, get all groups
        var groupsToCheck = targetGroups ??
            Enum.GetValues(typeof(BuildTargetGroup)).Cast<BuildTargetGroup>().ToArray();

        foreach (BuildTargetGroup group in groupsToCheck)
        {
            if (group == BuildTargetGroup.Unknown) { continue; }     // Skip invalid group

            // Check if the platform module is installed before processing
            if (!IsPlatformModuleInstalled(group))
            {
                Debug.Log($"Skipping {group} - Platform module not installed");
                continue;
            }

            try
            {
                var tmp = new HashSet<BuildValidationRule>();
                getIssues.Invoke(null, new object[] { tmp, group });

                foreach (var issue in tmp)
                {
                    // Check if issue message contains any ignored strings
                    bool shouldIgnore = false;
                    if (issue.Message != null)
                    {
                        if (ignoredStrings != null)
                        {
                            foreach (var ignoredString in ignoredStrings)
                            {
                                if (!string.IsNullOrEmpty(ignoredString) &&
                                    issue.Message.Contains(ignoredString))
                                {
                                    shouldIgnore = true;
                                    Debug.Log($"Ignoring issue: {issue.Message}");
                                    ignoredCount++;
                                    break;
                                }
                            }
                        }
                    }

                    if (!shouldIgnore) { allIssues.Add(issue); }
                }
            }
            catch (TargetInvocationException tie)
            {
                // OpenXR may not be configured for all build targets
                if (tie.InnerException is NullReferenceException)
                {
                    Debug.Log($"Skipping {group} - OpenXR not configured for this platform");
                }
                else
                {
                    Debug.LogWarning($"Error checking validation issues for {group}: {tie.InnerException?.Message}");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Unexpected error checking validation issues for {group}: {e.Message}");
            }
        }
        return (allIssues, ignoredCount);
    }

    /// <summary>
    /// Checks if a platform module is installed and available for building.
    /// </summary>
    /// <param name="targetGroup">The build target group to check.</param>
    /// <returns>True if the platform module is installed, false otherwise.</returns>
    private static bool IsPlatformModuleInstalled(BuildTargetGroup targetGroup)
    {
        // Map BuildTargetGroup to BuildTarget for the check
        BuildTarget buildTarget;
        switch (targetGroup)
        {
            case BuildTargetGroup.Standalone:
                // Standalone is always available
                return true;
            case BuildTargetGroup.Android:
                buildTarget = BuildTarget.Android;
                break;
            case BuildTargetGroup.iOS:
                buildTarget = BuildTarget.iOS;
                break;
            case BuildTargetGroup.VisionOS:
                buildTarget = BuildTarget.VisionOS;
                break;
            default:
                return false;
        }

        // Check if the build target is supported
        return BuildPipeline.IsBuildTargetSupported(targetGroup, buildTarget);
    }
}
