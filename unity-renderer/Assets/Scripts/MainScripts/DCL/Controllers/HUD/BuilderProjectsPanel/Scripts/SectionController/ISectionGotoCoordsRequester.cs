using System;
using UnityEngine;

internal interface ISectionGotoCoordsRequester
{
    event Action<Vector2Int>  OnRequestGoToCoords;
}