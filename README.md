# STYLY-XR-Rig

STYLY-XR-Rig is intended to help creators and STYLY developers to make experiences on multi platforms including Vision Pro.   

This XR Rig is currently used only in [STYLY for Vision Pro](https://apps.apple.com/us/app/styly-for-vision-pro/id6475184828) (not in other STYLY App for other HMDs)  

# How to add STYLY-XR-Rig

## Option 1

```
openupm add -f com.styly.styly-xr-rig
```

## Option 2 (If you use [STYLY-Spatial-Layer-Plugin](https://github.com/styly-dev/STYLY-Spatial-Layer-Plugin))
Right click on the hierarchy window to open the menu. Select `STYLY-XR-Rig` in the `XR` section.  
  
![How To Add STYLY XR Rig](https://github.com/styly-dev/STYLY-XR-Rig/assets/387880/e84dde8e-8000-48ec-b5bf-4492d9e6db97)

# Features
- visionOS support
  - Bounded guide gizmo for visionOS Bounded mode (requires PolySpatial)
  - Compatible with intractable objects with XR Interaction toolkit (XRI)
  - VisionOSHoverEffect will be automatically attached to Interactable GameObject (requires PolySpatial) 
  - Volume camera will be automatically created
  - Interaction Group will be automatically attached
- OpenXR Mixed Reality support
  - Video pass through will be enabled on OpenXR environment
- Unity Editor development support
  - Camera height is set to 1.3m only on Unity Editor
  - Camera height will be automatically set based on actual devices
  - XRI interaction works on Editor mode with a mouse without XR Simulator. (Currently supported only on visionOS target)
- AR Managers are included (disabled on start)
  - XR Interaction Manager
  - AR Mesh Manager
  - AR Plane Manager
  - AR Tracked Image Manager
  - AR Session will be automatically created if not exist
  - AR Anchor Manager (Directly attached to XR Origin and checked enabled)
- Others
  - Locomotion features are disabled because STYLY-XR-Rig is created for Spatial computing / Mixed Reality development
  - Other main-cameras and audio-listeners will be automatically disabled if exist in the scene

# Requirements
- Unity 6000.0 or higher
- Unity Pro license (optional, required only for PolySpatial features)
  - This XR-Rig is open source and provides core XRI functionality without any additional dependencies. PolySpatial is an optional dependency that enables additional visionOS-specific features like VisionOSHoverEffect and volume camera configuration, but is only available for Unity Pro developers.

# How the Rig is made
This section is written for maintenance purpose of this repository.

## Required samples installation (automatically)
Required packages and package samples are automatically installed into /Assets/Samples directory after you install this package or open the development project.
The required samples are listed in `required_samples.json`

- `XR Hands/x.x.x/HandVisualizer`
- `XR Interaction Toolkit/x.x.x/Hands Interaction Demo`
- `XR Interaction Toolkit/x.x.x/Starter Assets`
- `XR Interaction Toolkit/x.x.x/visionOS`

## STYLY-XR-Rig structure
Create `XR Origin Hands (XR Rig) Variant` from `Assets/Samples/XR Interaction Toolkit/x.x.x/Hands Interaction Demo/Prefabs/XR Origin Hands (XR Rig)` in `Packages/com.styly.styly-xr-rig/Runtime/Prefabs/XR Origin Hands (XR Rig) Variant.prefab`

Create prefab instance of `XR Origin Hands (XR Rig) Variant` under STYLY-XR-Rig

Add `ARAnchorManager.cs` to `XR Origin Hands (XR Rig) Variant` and set enabled. This manager script cannot be added at runtime.

Create empty prefab `_Added`
Create empty prefab `XRRigManager` as a child of `_Added` and attach scripts

- Scripts from XR Interaction Toolkit  
  - `Packages/com.unity.xr.interaction.toolkit/Runtime/Interaction/XRInteractionManager.cs`

- Scripts from /Runtime/Scripts  
  - `AddVisionOSHoverEffect.cs`
  - `ARMeshManagerAttacher.cs`
  - `ARPlaneManagerAttacher.cs`
  - `ARSessionCreator.cs`
  - `ARTrackedImageManagerAttacher.cs`
  - `DisableAnotherMainCameraAndAudioListener.cs`
  - `DisableLocomotion.cs`
  - `EnableOpenXrPassthrough.cs`
  - `SetCameraYOffsetZeroOnBuildApp.cs`
  - `InteractionGroupAttacher.cs`
    - Set PrimaryInteractionGroup prefab from XRI visionOS sample
    - Set SecondaryInteractionGroupVariant prefab from XRI visionOS sample
    - Set _added GameObject inside STYLY XR Rig
  - `HideHandMeshOnVisionOs.cs`
    - `Left Hand Visual`  
  - `PassthroughSupport.cs`
XR Origin Hands (XR Rig)  
└Camera Offset  
　└Left Hand  
　　└Left Hand Interaction Visual  
　　　└LeftHand  

    - `Left Pinch Visual`  
XR Origin Hands (XR Rig)  
└Camera Offset  
　└Left Hand  
　　└Pinch Point Stabilized  
　　　└Pinch Visual  

