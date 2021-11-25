using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DCL.Builder
{

    public class BuilderTest : MonoBehaviour
    { 
        private BuilderInWorldPlugin plugin;
        // Start is called before the first frame update
        void Start()
        {
            DataStore.i.featureFlags.flags.Set(new FeatureFlag());
            DataStore.i.builderInWorld.isDevBuild.Set(true);
            
            plugin = new BuilderInWorldPlugin();
            plugin.Initialize();
            plugin.panelController.SetVisibility(true);
            if (EventSystem.current == null)
            {
                var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }
        }

        // Update is called once per frame
        void Update() { }
    }

}
