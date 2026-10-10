#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Styly.XRRig;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;
using Object = UnityEngine.Object;

public class AndroidXROcclusionPermissionTests
{
    private const string SubsystemId = "STYLY-Permission-Test-Occlusion";
    private FakePermissions permissions;
    private XRGeneralSettings previousSettings;
    private XRGeneralSettings settings;
    private XRManagerSettings xrManager;
    private PermissionTestLoader loader;
    private XROcclusionSubsystem subsystem;
    private GameObject cameraObject;
    private SmartphoneARCameraManager cameraManager;

    [SetUp]
    public void SetUp()
    {
        Assert.That(Application.isPlaying, Is.True, "Run these lifecycle tests in PlayMode.");
        RequestAndroidXRPermissions.ResetSession();
        permissions = new FakePermissions();
        RequestAndroidXRPermissions.Backend = permissions;
        PermissionTestProvider.StartCalls = 0;
        PermissionTestProvider.StopCalls = 0;
        PermissionTestProvider.StartedWithoutPermission = false;
        PermissionTestProvider.IgnoreDepthMode = true;

        var descriptors = new List<XROcclusionSubsystemDescriptor>();
        SubsystemManager.GetSubsystemDescriptors(descriptors);
        if (!descriptors.Exists(descriptor => descriptor.id == SubsystemId))
        {
            XROcclusionSubsystemDescriptor.Register(new XROcclusionSubsystemDescriptor.Cinfo
            {
                id = SubsystemId,
                providerType = typeof(PermissionTestProvider),
                subsystemTypeOverride = typeof(PermissionTestSubsystem),
                environmentDepthImageSupportedDelegate = () => Supported.Supported
            });
            SubsystemManager.GetSubsystemDescriptors(descriptors);
        }
        subsystem = descriptors.Find(descriptor => descriptor.id == SubsystemId).Create();
        loader = ScriptableObject.CreateInstance<PermissionTestLoader>();
        loader.Occlusion = subsystem;
        previousSettings = XRGeneralSettings.Instance;
        settings = ScriptableObject.CreateInstance<XRGeneralSettings>();
        xrManager = ScriptableObject.CreateInstance<XRManagerSettings>();
        settings.Manager = xrManager;
        XRGeneralSettings.Instance = settings;
        // XR Management only permits pre-registered loaders in PlayMode. Register this
        // in-memory test loader without changing the project's saved XR settings.
        var registeredLoaders = (HashSet<XRLoader>)typeof(XRManagerSettings)
            .GetField("m_RegisteredLoaders", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(xrManager);
        registeredLoaders.Add(loader);
        Assert.That(xrManager.TryAddLoader(loader), Is.True);
        xrManager.InitializeLoaderSync();
        Assert.That(xrManager.activeLoader, Is.SameAs(loader));

        cameraObject = new GameObject("Permission Test Camera", typeof(Camera));
        cameraManager = cameraObject.AddComponent<SmartphoneARCameraManager>();
    }

    [TearDown]
    public void TearDown()
    {
        if (cameraObject != null) { Object.DestroyImmediate(cameraObject); }
        xrManager.DeinitializeLoader();
        subsystem.Destroy();
        XRGeneralSettings.Instance = previousSettings;
        Object.DestroyImmediate(loader);
        Object.DestroyImmediate(xrManager);
        Object.DestroyImmediate(settings);
        RequestAndroidXRPermissions.ResetSession();
    }

    [Test]
    public void PublicConfigurationPreservesEditorCameraBehavior()
    {
        cameraManager.ConfigureOcclusionSettings(SmartphoneARCameraManager.OcclusionSettings.AutomaticForMR);
        Assert.That(cameraObject.GetComponent<AROcclusionManager>(), Is.Null);
        Assert.That(cameraObject.GetComponent<ARCameraManager>(), Is.Null);
        Assert.That(permissions.RequestCalls, Is.Zero);
    }

    [Test]
    public void OtherProvidersDoNotRequestOrCheckAndroidXRPermissions()
    {
        permissions.IsAndroidXR = false;
        PermissionTestProvider.IgnoreDepthMode = false;

        cameraManager.ApplyOcclusionSettings(SmartphoneARCameraManager.OcclusionSettings.EnvironmentOnly);

        Assert.That(permissions.RequestCalls, Is.Zero);
        Assert.That(permissions.PermissionChecks, Is.Zero);
        Assert.That(PermissionTestProvider.StartCalls, Is.EqualTo(1));
        Assert.That(subsystem.requestedEnvironmentDepthMode, Is.EqualTo(EnvironmentDepthMode.Best));
        Assert.That(AndroidXRPermissions.UsesAndroidXR(loader), Is.False);
        Assert.That(AndroidXRPermissions.UsesAndroidXR(null), Is.False);
    }

    [TestCase(SmartphoneARCameraManager.OcclusionSettings.AutomaticForMR)]
    [TestCase(SmartphoneARCameraManager.OcclusionSettings.BothHumanAndEnvironment)]
    [TestCase(SmartphoneARCameraManager.OcclusionSettings.EnvironmentOnly)]
    public void OcclusionIsNotCreatedOrStartedUntilPermissionIsGranted(SmartphoneARCameraManager.OcclusionSettings mode)
    {
        cameraManager.ApplyOcclusionSettings(mode);

        Assert.That(cameraObject.GetComponent<AROcclusionManager>(), Is.Null);
        Assert.That(PermissionTestProvider.StartCalls, Is.Zero);

        permissions.Grant(RequestAndroidXRPermissions.HandTracking);
        Assert.That(PermissionTestProvider.StartCalls, Is.Zero);

        permissions.Grant(RequestAndroidXRPermissions.SceneUnderstandingFine);
        Assert.That(cameraObject.GetComponent<AROcclusionManager>().enabled, Is.True);
        Assert.That(PermissionTestProvider.StartCalls, Is.EqualTo(1));
        Assert.That(PermissionTestProvider.StartedWithoutPermission, Is.False);
    }

    [Test]
    public void DenialKeepsOcclusionStoppedWithoutRepeatedPrompts()
    {
        cameraManager.ApplyOcclusionSettings(SmartphoneARCameraManager.OcclusionSettings.AutomaticForMR);
        permissions.Deny(RequestAndroidXRPermissions.SceneUnderstandingFine);
        cameraManager.ApplyOcclusionSettings(SmartphoneARCameraManager.OcclusionSettings.EnvironmentOnly);

        Assert.That(cameraObject.GetComponent<AROcclusionManager>(), Is.Null);
        Assert.That(PermissionTestProvider.StartCalls, Is.Zero);
        Assert.That(permissions.RequestCalls, Is.EqualTo(1));
    }

    [TestCase(SmartphoneARCameraManager.OcclusionSettings.Disabled)]
    [TestCase(SmartphoneARCameraManager.OcclusionSettings.AutomaticForVR)]
    [TestCase(SmartphoneARCameraManager.OcclusionSettings.HumanOnly)]
    public void GrantUsesLatestModeInsteadOfStartingDepthInVR(SmartphoneARCameraManager.OcclusionSettings mode)
    {
        cameraManager.ApplyOcclusionSettings(SmartphoneARCameraManager.OcclusionSettings.AutomaticForMR);
        cameraManager.ApplyOcclusionSettings(mode);
        permissions.Grant(RequestAndroidXRPermissions.SceneUnderstandingFine);

        Assert.That(cameraObject.GetComponent<AROcclusionManager>(), Is.Null);
        Assert.That(PermissionTestProvider.StartCalls, Is.Zero);
    }

    [Test]
    public void ModeSwitchStopsAndRestartsAProviderThatIgnoresDepthMode()
    {
        permissions.Allowed.Add(RequestAndroidXRPermissions.SceneUnderstandingFine);
        cameraManager.ApplyOcclusionSettings(SmartphoneARCameraManager.OcclusionSettings.AutomaticForMR);
        Assert.That(subsystem.requestedEnvironmentDepthMode, Is.EqualTo(EnvironmentDepthMode.Disabled));
        Assert.That(subsystem.running, Is.True);

        cameraManager.ApplyOcclusionSettings(SmartphoneARCameraManager.OcclusionSettings.AutomaticForVR);
        Assert.That(subsystem.running, Is.False);
        Assert.That(PermissionTestProvider.StopCalls, Is.EqualTo(1));

        cameraManager.ApplyOcclusionSettings(SmartphoneARCameraManager.OcclusionSettings.AutomaticForMR);
        Assert.That(subsystem.running, Is.True);
        Assert.That(PermissionTestProvider.StartCalls, Is.EqualTo(2));
        Assert.That(PermissionTestProvider.StartedWithoutPermission, Is.False);
    }

    [Test]
    public void GrantWhileDisabledWaitsUntilCameraManagerIsEnabled()
    {
        cameraManager.ApplyOcclusionSettings(SmartphoneARCameraManager.OcclusionSettings.AutomaticForMR);
        cameraManager.enabled = false;
        permissions.Grant(RequestAndroidXRPermissions.SceneUnderstandingFine);
        Assert.That(PermissionTestProvider.StartCalls, Is.Zero);

        cameraManager.enabled = true;
        Assert.That(PermissionTestProvider.StartCalls, Is.EqualTo(1));
    }

    [Test]
    public void ReturningFromSettingsAppliesGrantsAndRevocations()
    {
        cameraManager.ApplyOcclusionSettings(SmartphoneARCameraManager.OcclusionSettings.AutomaticForMR);
        permissions.Allowed.Add(RequestAndroidXRPermissions.SceneUnderstandingFine);
        cameraObject.SendMessage("OnApplicationFocus", true);
        Assert.That(subsystem.running, Is.True);

        permissions.Allowed.Clear();
        cameraObject.SendMessage("OnApplicationFocus", true);
        Assert.That(subsystem.running, Is.False);
        Assert.That(permissions.RequestCalls, Is.EqualTo(1));
    }

    [Test]
    public void LateGrantDoesNotAccessADestroyedCamera()
    {
        cameraManager.ApplyOcclusionSettings(SmartphoneARCameraManager.OcclusionSettings.AutomaticForMR);
        Object.DestroyImmediate(cameraObject);

        Assert.DoesNotThrow(() => permissions.Grant(RequestAndroidXRPermissions.SceneUnderstandingFine));
        Assert.That(PermissionTestProvider.StartCalls, Is.Zero);
    }

    [Test]
    public void RequestsOnlyMissingPermissionsOnce()
    {
        permissions.Allowed.Add(RequestAndroidXRPermissions.HandTracking);
        RequestAndroidXRPermissions.RequestPermissions();
        RequestAndroidXRPermissions.RequestPermissions();

        Assert.That(permissions.Requested, Is.EqualTo(new[] { RequestAndroidXRPermissions.SceneUnderstandingFine }));
        Assert.That(permissions.RequestCalls, Is.EqualTo(1));
    }

    [Test]
    public void DoesNotPromptWhenPermissionsAreAlreadyGranted()
    {
        permissions.Allowed.Add(RequestAndroidXRPermissions.HandTracking);
        permissions.Allowed.Add(RequestAndroidXRPermissions.SceneUnderstandingFine);
        cameraManager.ApplyOcclusionSettings(SmartphoneARCameraManager.OcclusionSettings.EnvironmentOnly);

        Assert.That(permissions.RequestCalls, Is.Zero);
        Assert.That(PermissionTestProvider.StartCalls, Is.EqualTo(1));
    }

    [Test]
    public void AnAbsentStartupProviderDoesNotConsumeThePermissionRequest()
    {
        permissions.IsAndroidXR = false;
        RequestAndroidXRPermissions.RequestPermissions();
        permissions.IsAndroidXR = true;
        RequestAndroidXRPermissions.RequestPermissions();

        Assert.That(permissions.RequestCalls, Is.EqualTo(1));
    }

    private sealed class FakePermissions : IAndroidXRPermissions
    {
        public bool IsAndroidXR { get; set; } = true;
        public readonly HashSet<string> Allowed = new HashSet<string>();
        public int RequestCalls;
        public int PermissionChecks;
        public string[] Requested;
        private Action<string> onGranted;
        private Action<string> onDenied;

        public bool IsGranted(string permission)
        {
            PermissionChecks++;
            return Allowed.Contains(permission);
        }

        public void Request(string[] requested, Action<string> granted, Action<string> denied)
        {
            RequestCalls++;
            Requested = requested;
            onGranted = granted;
            onDenied = denied;
        }

        public void Grant(string permission)
        {
            Allowed.Add(permission);
            onGranted(permission);
        }

        public void Deny(string permission) => onDenied(permission);
    }

    public sealed class PermissionTestLoader : XRLoader
    {
        public XROcclusionSubsystem Occlusion;
        public override bool Initialize() => true;
        public override bool Deinitialize() => true;
        public override T GetLoadedSubsystem<T>() => Occlusion as T;
    }

    public sealed class PermissionTestSubsystem : XROcclusionSubsystem { }

    public sealed class PermissionTestProvider : XROcclusionSubsystem.Provider
    {
        public static int StartCalls;
        public static int StopCalls;
        public static bool StartedWithoutPermission;
        public static bool IgnoreDepthMode;
        private EnvironmentDepthMode mode;

        protected override bool TryInitialize() => true;

        public override void Start()
        {
            StartCalls++;
            StartedWithoutPermission |= !RequestAndroidXRPermissions.IsGranted(RequestAndroidXRPermissions.SceneUnderstandingFine);
        }

        public override void Stop() => StopCalls++;
        public override void Destroy() { }

        public override EnvironmentDepthMode requestedEnvironmentDepthMode
        {
            get => IgnoreDepthMode ? EnvironmentDepthMode.Disabled : mode;
            set { if (!IgnoreDepthMode) { mode = value; } }
        }
    }
}
#endif
