using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlaceCategoryIcons", menuName = "Variables/PlaceCategoryIcons")]
public class PlaceCategoryIcons : ScriptableObject
{
    public PlaceCategoryIcon[] categoryIcons;
}

[Serializable]
public class PlaceCategoryIcon
{
    public string category;
    public Sprite icon;
}
