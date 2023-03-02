using UnityEngine;


[CreateAssetMenu(fileName = "NewItemToggleSkin", menuName = "DCL/ItemToggleSkin")]
public class NFTItemToggleSkin : ScriptableObject
{
    public Color backgroundColor;
    public Color rarityNameColor;
    public Color gradientColor;
    public bool isBase;
}
