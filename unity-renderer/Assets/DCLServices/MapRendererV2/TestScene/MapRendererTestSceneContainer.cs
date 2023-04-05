using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("MapRendererTestSceneEditor")]

namespace DCLServices.MapRendererV2.TestScene
{
    public class MapRendererTestSceneContainer : MonoBehaviour
    {
        [field: SerializeField]
        internal MapRendererTestSceneHotScenesController hotScenesController { get; private set; }
    }
}
