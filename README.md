# STYLY-XR-Rig

STYLY-XR-Rig is intended to help developers to make XR experiences on multiple platforms.   

# How to add STYLY-XR-Rig

## Add package

```
openupm add -f com.styly.styly-xr-rig
```

## Add STYLY XR Rig to your scene
Right click on the hierarchy window to open the menu. Select `STYLY-XR-Rig` in the `XR` section.  
  
![How To Add STYLY XR Rig](https://github.com/styly-dev/STYLY-XR-Rig/assets/387880/e84dde8e-8000-48ec-b5bf-4492d9e6db97)

## Setup device SDK
Just click target SDK on the menu. The SDK will be downloaded and installed. The appropriate settings are automatically configured. See more details [here](https://github.com/styly-dev/STYLY-XR-Rig/tree/develop/Packages/com.styly.styly-xr-rig/Editor/SetupSDK).

<img width="507" height="243" alt="menu" src="https://github.com/user-attachments/assets/cb953854-8d16-4994-822a-f9c8e6c71293" />

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
  - WASD movement in the editor mode.
  - Stick control with VR controllers.
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
Created `XR Origin Hands (XR Rig) Variant` from `Assets/Samples/XR Interaction Toolkit/x.x.x/Hands Interaction Demo/Prefabs/XR Origin Hands (XR Rig)` in `Packages/com.styly.styly-xr-rig/Runtime/Prefabs/XR Origin Hands (XR Rig) Variant.prefab`
Renamed the variant to `STYLY XR Rig`

Added scripts and components to the Rig prefab. The changes can be viewed on the prefab overrides menu.

<img width="464" height="461" alt="STYLYXRRig" src="https://github.com/user-attachments/assets/83298b52-1c12-4b3e-ae9a-1ace8bbffa9f" />

<img width="862" height="564" alt="STYLYXRRigManager" src="https://github.com/user-attachments/assets/8c8b4925-a231-4fff-afda-5975f0c37443" />

