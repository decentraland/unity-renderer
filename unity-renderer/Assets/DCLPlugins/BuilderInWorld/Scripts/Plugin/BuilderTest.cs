using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DCL.Builder
{
    public class BuilderTest : MonoBehaviour
    {
        private BuilderInWorldPlugin plugin;

        void Start()
        {
            DataStore.i.featureFlags.flags.Set(new FeatureFlag());
            DataStore.i.builderInWorld.isDevBuild.Set(true);

            plugin = new BuilderInWorldPlugin();
            plugin.Enable();
            plugin.panelController.SetVisibility(true);

            if (EventSystem.current == null)
            {
                var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }
        }
    }
}