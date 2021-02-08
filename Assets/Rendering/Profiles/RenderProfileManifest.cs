using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL
{
    /// <summary>
    /// RenderProfileManifest is used to store and set the current used RenderProfileWorld.
    ///
    /// When a new RenderProfileWorld object is added to the project it must be added here as well.
    /// </summary>
    [CreateAssetMenu(menuName = "DCL/Rendering/Render Profile Manifest", fileName = "RenderProfileManifest", order = 0)]
    public class RenderProfileManifest : ScriptableObject
    {
        private static RenderProfileManifest instance;
        public static RenderProfileManifest i => GetOrLoad(ref instance, "Render Profile Manifest");

        public RenderProfileWorld defaultProfile;
        public RenderProfileWorld nightProfile;
        public RenderProfileWorld testProfile;

        private RenderProfileWorld currentProfileValue;

        public RenderProfileWorld currentProfile
        {
            get { return currentProfileValue; }
            set
            {
                if (value == null || value == currentProfileValue)
                    return;

                currentProfileValue = value;
                OnChangeProfile?.Invoke(value);
            }
        }

        public event Action<RenderProfileWorld> OnChangeProfile;

        internal static T GetOrLoad<T>(ref T variable, string path) where T : Object
        {
            if (variable == null)
            {
                variable = Resources.Load<T>(path);
            }

            return variable;
        }

        public void Initialize(RenderProfileWorld initProfile = null)
        {
            currentProfile = initProfile != null ? initProfile : defaultProfile;

            if (currentProfile == null)
            {
                Debug.Log("Default profile should never be null!");
                return;
            }

            currentProfile.avatarProfile.currentProfile = currentProfile.avatarProfile.inWorld;
            currentProfile.Apply();
        }
    }
}