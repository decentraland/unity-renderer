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

            if (EventSystem.current == null)
            {
                var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }

            BuilderInWorldLoadingController controller = new BuilderInWorldLoadingController();
            controller.Initialize();
            controller.Show();
            controller.SetPercentage(50f);
        }
    }
}