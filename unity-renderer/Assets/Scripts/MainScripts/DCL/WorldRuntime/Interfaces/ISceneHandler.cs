using DCL.Controllers;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public interface ISceneHandler
    {
        HashSet<Vector2Int> GetAllLoadedScenesCoords();
        HashSet<Vector2Int> GetEmptyLoadedSceneCoords();
        IParcelScene GetScene(Vector2Int coords); 
    }
}