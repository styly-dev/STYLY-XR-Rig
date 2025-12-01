using System;
using UnityEngine;

namespace Styly.XRRig
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Gets the component of type T from the GameObject, or adds it if it doesn't exist.
        /// </summary>
        /// <example>
        /// var comp = myGO.GetOrAddComponent&lt;MyComponent&gt;();
        /// </example>
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            if (go == null) throw new ArgumentNullException(nameof(go));
            T comp = go.GetComponent<T>();
            return comp ?? go.AddComponent<T>();
        }

        /// <summary>
        /// Gets the component of type T from the GameObject, or adds it and initializes it if it doesn't exist.
        /// </summary>
        /// <example>
        /// var comp2 = myGO.GetOrAddComponent&lt;MyComponent&gt;(c => {
        ///     c.someValue = 42;
        ///     c.enabled = true;
        /// });
        /// </example>
        public static T GetOrAddComponent<T>(this GameObject go, Action<T> initializer) where T : Component
        {
            if (go == null) throw new ArgumentNullException(nameof(go));
            T comp = go.GetComponent<T>();
            if (comp == null)
            {
            comp = go.AddComponent<T>();
            initializer?.Invoke(comp);
            }
            return comp;
        }

        /// <summary>
        /// Tries to get the first component of type T in the GameObject's children.
        /// </summary>
        public static bool TryGetComponentInChildren<T>(this GameObject go, out T component, bool includeInactive = false) where T : Component
        {
            component = default;
            if (go == null) return false;
            T[] comps = go.GetComponentsInChildren<T>(includeInactive);
            if (comps != null && comps.Length > 0)
            {
                component = comps[0];
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the layer recursively for the GameObject and all its descendants.
        /// </summary>
        public static void SetLayerRecursively(this GameObject go, int layer)
        {
            if (go == null) return;
            var t = go.transform;
            // stack-based traversal to avoid recursive function overhead for deep hierarchies
            var stack = new System.Collections.Generic.Stack<Transform>();
            stack.Push(t);
            while (stack.Count > 0)
            {
                var cur = stack.Pop();
                cur.gameObject.layer = layer;
                for (int i = 0; i < cur.childCount; i++)
                {
                    stack.Push(cur.GetChild(i));
                }
            }
        }

        /// <summary>
        /// Sets the active state of the GameObject only if it differs from the current state.
        /// </summary>
        public static void SetActiveIfChanged(this GameObject go, bool active)
        {
            if (go == null) return;
            if (go.activeSelf != active) go.SetActive(active);
        }

        /// <summary>
        /// Destroys all immediate children of the GameObject.
        /// </summary>
        public static void DestroyChildren(this GameObject go, bool allowImmediate = false)
        {
            if (go == null) return;
            // collect first to avoid modification-during-iteration issues
            int n = go.transform.childCount;
            var children = new Transform[n];
            for (int i = 0; i < n; i++) children[i] = go.transform.GetChild(i);

            for (int i = 0; i < children.Length; i++)
            {
                var childGO = children[i].gameObject;
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(childGO);
                }
                else
                {
                    // In Editor, prefer DestroyImmediate if allowed; otherwise DestroyImmediate is fine only in editor.
                    if (allowImmediate)
                        UnityEngine.Object.DestroyImmediate(childGO);
                    else
                        UnityEngine.Object.Destroy(childGO);
                }
            }
        }

        /// <summary>
        /// Finds a child GameObject by name, optionally searching recursively.
        /// </summary>
        public static GameObject FindChildByName(this GameObject go, string name, bool recursive = true)
        {
            if (go == null || string.IsNullOrEmpty(name)) return null;
            var t = go.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                var child = t.GetChild(i);
                if (child.name == name) return child.gameObject;
                if (recursive)
                {
                    var found = child.gameObject.FindChildByName(name, true);
                    if (found != null) return found;
                }
            }
            return null;
        }

        /// <summary>
        /// Resets the local transform to identity (position 0, rotation 0, scale 1).
        /// </summary>
        public static void ResetLocalTransform(this Transform t)
        {
            if (t == null) return;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        /// <summary>
        /// Enables or disables all components of type T in the GameObject and its children.
        /// </summary>
        public static void EnableComponents<T>(this GameObject go, bool enabled) where T : Behaviour
        {
            if (go == null) return;
            var comps = go.GetComponentsInChildren<T>(true);
            foreach (var c in comps) if (c != null) c.enabled = enabled;
        }

        /// <summary>
        /// Gets the first component of type T in the GameObject's children, or adds it to the GameObject if not found.
        /// </summary>
        public static T GetOrAddComponentInChildren<T>(this GameObject go, bool includeInactive = false) where T : Component
        {
            if (go == null) throw new ArgumentNullException(nameof(go));
            var comps = go.GetComponentsInChildren<T>(includeInactive);
            if (comps != null && comps.Length > 0) return comps[0];
            return go.AddComponent<T>();
        }
    }
}
