using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    [CreateAssetMenu(fileName = "EditorToolSize", menuName = "ScriptableObjects/SkyboxEditorSize", order = 1)]
    public class EditorToolMeasurements : ScriptableObject
    {
        public GUIStyle testStyle;
        public float topPanelTopStart = 5;
        public float topPanelLeftStart = 5;
        public float toolRightPadding = 5;
        public float topPanelHeight = 200;
        public Color panelBGColor = Color.grey;
        public float panelsPadding = 10;
        [Range(0.0f, 1f)]
        public float leftPanelWidthPercentage = 0.25f;

        // Left Panel
        public Rect leftPanelPadding;
        public float leftPanelButtonSpace;

        // Right Panel
        [Header("Right Panel")]
        public Rect rightPanelPadding;
    }
}