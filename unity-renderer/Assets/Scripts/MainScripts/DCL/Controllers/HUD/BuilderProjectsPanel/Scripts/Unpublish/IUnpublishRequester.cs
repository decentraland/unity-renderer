using System;
using UnityEngine;

internal interface IUnpublishRequester
{
    event Action<Vector2Int> OnRequestUnpublish;
}