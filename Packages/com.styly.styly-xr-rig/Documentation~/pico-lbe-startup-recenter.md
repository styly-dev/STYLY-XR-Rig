# PICO LBE startup recenter

Enable **Recenter Pico Lbe On Startup** on the `StylyXrRig` component to request
one PICO Enterprise recenter during app startup. It is **off by default**. This
is an opt-in workaround for intermittent LBE startup misalignment, not a fix
for the underlying runtime or SDK.

## Requirements and behavior

- Android player with the PICO OpenXR SDK installed through the existing PICO
  setup flow. The optional adapter does not add a PICO dependency to other builds.
  Without the SDK, the setting is ignored with a skip log; no PICO initialization
  or recenter calls are made, even when the option is enabled.
- A PICO OpenXR runtime and a successfully initialized/bound Enterprise Service.
- An enabled LBE switch, checked again immediately before the recenter request.
- A focused application, a running XR input subsystem in Floor mode, recenter
  support enabled, and a tracked HMD with valid position and rotation.

Awake schedules preparation; platform calls start after scene initialization.
The operation waits asynchronously and does not block passthrough setup. It calls
`PXR_Enterprise.Recenter()`, which PICO documents as equivalent to holding Home.
It never changes the origin mode, recenter policy, LBE switch, button mappings,
or rig transforms.

**Pico Lbe Recenter Timeout Seconds** bounds the entire preparation (default
15 seconds, unscaled). Disabling/destroying the requesting rig cancels the
request. Late callbacks cannot trigger a recenter. All Unity/SDK calls execute
on the Unity main thread. The shared Enterprise Service is not unbound by this
feature.

Only the first opted-in rig can attempt this operation during an app session,
including failed, timed-out, or cancelled attempts. Recreating a rig, changing
scenes, or regaining focus does not retry it. Editor Play sessions reset the
guard, including when domain reload is disabled. The Editor does not call PICO
Enterprise APIs.

Outcomes use the log prefix `[STYLY XR Rig] PICO LBE startup recenter:` and report
Completed, Skipped, Failed, TimedOut, or Cancelled. `Recenter` result 0 means the
API accepted the request; it does not prove physical alignment.

## Integration and validation

Keep this setting disabled when the application already owns startup recentering
or Enterprise Service binding. PICO's binding callback is shared, so independently
binding the service concurrently can interfere with callbacks; coordinate that
initialization in the application instead of enabling two owners.

An enabled LBE switch and valid head tracking do not prove that the saved map has
localized. Verify physical markers across cold launches, app-only restarts, and
different positions/headings on each supported device/OS. Sleep/resume recovery
is intentionally outside this startup-only feature.

The repository's EditMode tests exercise preparation ordering, disabled/unknown
LBE, failed SDK calls, timeout/cancellation, late callbacks, readiness loss, and
the per-session guard. They do not establish headset alignment.

References: [PICO LBE guidance](https://business.picoxr.com/es/doc/Enterprise-LBE),
[PICO Recenter API](https://github.com/Pico-Developer/PICO-Unity-OpenXR-SDK/blob/3aa3e62bff41df618529eeb60ff02c29a515dafe/Enterprise/Scripts/PXR_Enterprise.cs#L3325-L3333).
