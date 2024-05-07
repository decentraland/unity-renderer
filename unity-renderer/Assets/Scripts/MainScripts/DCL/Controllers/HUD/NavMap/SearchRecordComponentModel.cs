using System;
using UnityEngine;

[Serializable]
public record SearchRecordComponentModel
{
    public string recordText;
    public bool isHistory;
    public int playerCount;
    public Vector2Int placeCoordinates;
}
