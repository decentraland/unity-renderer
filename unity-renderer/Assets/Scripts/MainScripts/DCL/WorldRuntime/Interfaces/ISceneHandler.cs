using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public interface ISceneHandler
    {
        HashSet<Vector2Int> GetAllLoadedScenesCoords();
    }
}