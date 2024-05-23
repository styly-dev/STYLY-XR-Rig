# STYLY-XR-Rig

STYLY-XR-Rig is intended to help creators and STYLY developers to make experiences on multi platforms including Vision Pro.   

This XR Rig is currently used only in [STYLY for Vision Pro](https://apps.apple.com/us/app/styly-for-vision-pro/id6475184828) (not in other STYLY App for other HMDs)  

# How to add STYLY-XR-Rig

Right click on the hierarchy window to open the menu. Select `STYLY-XR-Rig` in the `XR` section.  
  
![How To Add STYLY XR Rig](https://github.com/styly-dev/STYLY-XR-Rig/assets/387880/e84dde8e-8000-48ec-b5bf-4492d9e6db97)

# Features
- visionOS support
  - Bounded guide gizmo for visionOS Bounded mode
  - Compatible with intractable objects with XR Interaction toolkit (XRI)
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
- Others
  - Locomotion features are disabled because STYLY-XR-Rig is created for Spatial computing / Mixed Reality development
  - Other main-cameras and audi-listeners will be automatically disabled if exist in the scene

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
Copy `XR Origin Hands (XR Rig)` from `Assets/Samples/XR Interaction Toolkit/x.x.x/Hands Interaction Demo/Prefabs/`

Create empty prefab `_Added`

Copy two prefabs as children of `_Added` from `Assets/Samples/XR Interaction Toolkit/3.0.3/visionOS/Prefabs/Interactors`
- `Primary Interaction Group`
- `Secondary Interaction Group Variant`

Create empty prefab `XRRigManager` as a child of `_Added` and attach scripts

- Scripts from XR Interaction Toolkit  
  - `Packages/com.unity.xr.interaction.toolkit/Runtime/Interaction/XRInteractionManager.cs`

- Scripts from /Runtime/Scripts  
  - `ARMeshManagerAttacher.cs`
  - `ARPlaneManagerAttacher.cs`
  - `ARSessionCreator.cs`
  - `ARTrackedImageManagerAttacher.cs`
  - `DisableAnotherMainCameraAndAudioListener.cs`
  - `DisableLocomotion.cs`
  - `EnableOpenXrPassthrough.cs`
  - `SetCameraYOffsetZeroOnBuildApp.cs`
  - `HideHandMeshOnVisionOs.cs`
    - `Left Hand Visual`  
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

Create empty prefab `VolumeCamera` as a child of `_Added` and attach scripts
- `VolumeCamera.cs` Set dimensions to (1,1,1)
