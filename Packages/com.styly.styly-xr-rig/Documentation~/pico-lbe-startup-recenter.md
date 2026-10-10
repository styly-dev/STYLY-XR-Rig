# PICO LBE startup recenter

`StylyXrRig` automatically prepares one PICO Enterprise recenter during app
startup when the requirements below are met. It is **enabled by default** and
has no Inspector settings. This is a workaround for intermittent LBE startup
misalignment; physical alignment still requires validation on the target device.

## Requirements and behavior

- Android player with the PICO OpenXR SDK installed through the existing PICO
  setup flow. The optional adapter does not add a PICO dependency to other builds.
  Without the SDK, preparation is silently skipped: no PICO initialization or
  recenter calls are made. Editor and non-Android builds also skip preparation.
- A PICO OpenXR runtime and a successfully initialized/bound Enterprise Service.
- An enabled LBE switch, checked again immediately before the recenter request.
- A focused application, a running XR input subsystem in Floor mode, recenter
  support enabled, and a tracked HMD with valid position and rotation.

Awake schedules preparation; platform calls start after scene initialization.
The operation waits asynchronously and does not block passthrough setup. It calls
`PXR_Enterprise.Recenter()`, which PICO documents as equivalent to holding Home.
It never changes the origin mode, recenter policy, LBE switch, button mappings,
or rig transforms.

The entire preparation has a fixed timeout of 15 seconds, unscaled. Disabling or
destroying the requesting rig cancels the request. Late callbacks cannot trigger
a recenter. All Unity/SDK calls execute
on the Unity main thread. The shared Enterprise Service is not unbound by this
feature.

Only the first eligible rig can attempt this operation during an app session,
including failed, timed-out, or cancelled attempts. Recreating a rig, changing
scenes, or regaining focus does not retry it. Editor Play sessions reset the
guard, including when domain reload is disabled. The Editor does not call PICO
Enterprise APIs.

Attempt outcomes use the log prefix `[STYLY XR Rig] PICO LBE startup recenter:`
and report Completed, Skipped, Failed, TimedOut, or Cancelled. `Recenter` result 0 means the
API accepted the request; it does not prove physical alignment.

## Disable from code

Set `PicoLbeStartupRecenter.Enabled = false` on the Unity main thread before the
first rig's Awake. Use a `BeforeSceneLoad` callback so this runs after the
per-session reset and before scene initialization, regardless of Awake ordering:

```csharp
using Styly.XRRig;
using UnityEngine;

public static class AppXrStartup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Configure()
    {
        PicoLbeStartupRecenter.Enabled = false;
    }
}
```

This disables preparation for all rigs in the app session, including Enterprise
Service initialization. Setting it to false during preparation cancels the
pending attempt on its next tick, before processing callbacks or recentering.
Re-enabling does not restart a cancelled or completed attempt, or undo a recenter
that already happened. The property resets to true for each app/Editor Play
session, including when domain reload is disabled.

## Integration and validation

Use the code opt-out when the application already owns startup recentering or
Enterprise Service binding. PICO's binding callback is shared, so independently
binding the service concurrently can interfere with callbacks. Checking LBE
status itself requires a service connection, even when LBE turns out to be off.
Coordinate initialization in the application instead of enabling two owners.

An enabled LBE switch and valid head tracking do not prove that the saved map has
localized. Verify physical markers across cold launches, app-only restarts, and
different positions/headings on each supported device/OS. Sleep/resume recovery
is intentionally outside this startup-only feature.

The repository's EditMode tests exercise automatic startup, code opt-out before
and during preparation, missing SDK, disabled/unknown LBE, failed SDK calls,
timeout/cancellation, late callbacks, readiness loss, and the per-session guard.
They do not establish headset alignment.

References: [PICO LBE guidance](https://business.picoxr.com/es/doc/Enterprise-LBE),
[PICO Recenter API](https://github.com/Pico-Developer/PICO-Unity-OpenXR-SDK/blob/3aa3e62bff41df618529eeb60ff02c29a515dafe/Enterprise/Scripts/PXR_Enterprise.cs#L3325-L3333).
