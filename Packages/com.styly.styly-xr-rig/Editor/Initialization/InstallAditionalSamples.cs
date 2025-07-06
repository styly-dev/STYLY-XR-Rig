using UnityEngine;
using UnityEditor;

namespace Styly.XRRig
{
    public class InstallAditionalSamples
    {
        [InitializeOnLoadMethod]
        private static void InstallSamples()
        {
#if USE_POLYSPATIAL
            // Install visionOS sample if PolySpatial is installed
            InstallRequiredSamples.InstallSample("com.unity.xr.interaction.toolkit","visionOS");
#endif
        }
    }
}