using System.Collections.Generic;
using UnityEditor;

namespace Styly.XRRig.Editor
{
    [CustomEditor(typeof(StylyXrRig))]
    public class StylyXrRigEditor : UnityEditor.Editor
    {
        private static readonly HashSet<string> VisionOsPropertyNames = new()
        {
            "UseBoundedModeForVisionOs",
            "BoundedVolumeCameraObj",
            "UnBoundedVolumeCameraObj",
            "CameraOffset",
            "CameraHeightInEditor"
        };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            HideVisionFieldsForNonVisionOsPlatform();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Hide visionOS specific fields when the current platform is not visionOS.
        /// </summary>
        private void HideVisionFieldsForNonVisionOsPlatform()
        {
            bool hideVisionFields = !Utils.IsVisionOS();
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                bool isScriptField = iterator.propertyPath == "m_Script";
                if (hideVisionFields && VisionOsPropertyNames.Contains(iterator.propertyPath))
                {
                    continue;
                }

                EditorGUI.BeginDisabledGroup(isScriptField);
                EditorGUILayout.PropertyField(iterator, true);
                EditorGUI.EndDisabledGroup();
            }
        }
    }
}
