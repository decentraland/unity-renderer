using UnityEngine;

[CreateAssetMenu(menuName = "DCL/Create OutlinerConfig", fileName = "OutlinerConfig", order = 0)]
public class OutlinerConfig : ScriptableObject
{
    // Outline draw method enum

    [Header("Outline Rendering Methods")]
    [Space]
    [SerializeField] public OutlineRenderMethod outlineRenderMethod = OutlineRenderMethod.OutlineStandard;

    [Header("Outline Properties")]
    [Space]
    // outline color width , amount
    [SerializeField] public Color outlineColor = Color.green;
    [SerializeField] [Range(1f, 10f)] public float outlineWidth = 2f;
    [SerializeField] [Range(0f, 1f)] public float outlineAmount = 1f;

    [Space]
    // overlay color
    [SerializeField] public Color overlayColor = Color.black;
    // overlay amount
    [SerializeField] [Range(0f, 1f)] public float overlayAmount = 1f;

    [Space]
    // vertex color sensitive case
    [SerializeField] public bool vertexColorRedChannel = false;

    // outline shader
    [SerializeField] public Shader shaderOutline;
}