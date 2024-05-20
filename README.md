# STYLY-XR-Rig

STYLY-XR-Rig is intended to help creators and STYLY developers to make experiences on multi platforms including Vision Pro.   

This XR Rig is currently used only in [STYLY for Vision Pro](https://apps.apple.com/us/app/styly-for-vision-pro/id6475184828) (not in other STYLY App for other HMDs)  


# How to add STYLY-XR-Rig

Right click on the hierarchy window to open the menu. Select `STYLY-XR-Rig` in the `XR` section.  
  
![How To Add STYLY XR Rig](https://github.com/styly-dev/STYLY-XR-Rig/assets/387880/e84dde8e-8000-48ec-b5bf-4492d9e6db97)


# How the Rig is made
This section is written for maintenance purose of this repository.

## Required samples installation (automatticaly)
Required packages and package samples are automatically installed into /Assets/Samples directory after you install this package or open the development project.
The required samples are listed in `required_samples.json`

- `XR Hands/x.x.x/HandVisualizer`
- `XR Interaction Toolkit/x.x.x/Hands Interaction Demo`
- `XR Interaction Toolkit/x.x.x/Starter Assets`
- `XR Interaction Toolkit/x.x.x/visionOS`

## STYLY-XR-Rig structure
Copy `XR Origin Hands (XR Rig)` from `Assets/Samples/XR Interaction Toolkit/x.x.x/Hands Interaction Demo/Prefabs/`

Create empty prefab `_Added` as a child of `XR Origin Hands (XR Rig)`  

Copy two prefabs as children of `_Added` from `Assets/Samples/XR Interaction Toolkit/3.0.3/visionOS/Prefabs/Interactors`
- `Primary Interaction Group`
- `Secondary Interaction Group Variant`

Add Bounded guide as a child of `_Added`
- `Bounded Guide Frame`

Create empty prefab `XRRigManager` as a child of `_Added` and attach scripts
- `XRInteractionManager.cs`
- `ARSessionCreator.cs`
- `ARTrackedImageManagerAttacher.cs`
- `ARMeshManagerAttacher.cs`
- `HideHandMeshOnVisionOs.cs`
- `EnableOpenXrPassthrough.cs`
- `SetCameraYOffsetZeroOnBuildApp.cs`
- `DisableLocomotion.cs`
- `DisableAnotherMainCameraAndAudioListener.cs`
- `ARPlaneManagerAttacher.cs`

Create empty prefab `VolumeCamera` as a child of `_Added` and attach scripts
- `VolumeCamera.cs`
- `MoveVolumeCameraToUnderCameraOffset.cs`



