using System;
using UnityEngine;

internal interface ISectionEditSceneAtCoordsRequester
{
    event Action<Vector2Int> OnRequestEditSceneAtCoords;
}