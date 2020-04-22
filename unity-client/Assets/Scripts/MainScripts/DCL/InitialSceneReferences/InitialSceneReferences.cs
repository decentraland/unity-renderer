using DCL.Helpers;
using UnityEngine;

namespace DCL
{
    public class InitialSceneReferences : MonoBehaviour
    {
        [SerializeField] private MouseCatcher mouseCatcherReference;
        [SerializeField] private GameObject groundVisualReference;

        public GameObject groundVisual { get { return groundVisualReference; } }
        public MouseCatcher mouseCatcher { get { return mouseCatcherReference; } }


        public static InitialSceneReferences i { get; private set; }

        void Awake()
        {
            if (i != null)
            {
                Utils.SafeDestroy(this);
                return;
            }

            i = this;
        }

        void OnDestroy()
        {
            i = null;
        }
    }
}
