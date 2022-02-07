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
        public float layerButtonWidth = 50;
        public float layerRenderWidth = 20;
        public float layerActiveCheckboxSize = 15;
        public GUIStyle leftPanelHorizontal;

        // Right Panel
        [Header("Right Panel")]
        public float pinnedPanelHeight = 300;
        public Rect pinnedPanelBGOffset;
        public Rect rightPanelPadding;
        public GUIStyleStateVar rightPanelHeadingState;
        public GUIStyleStateVar rightPanelHeadingTextColor;

        private void OnValidate()
        {
            // Make new texture and assign to specific style state
            rightPanelHeadingState.AssignTexture();
            rightPanelHeadingTextColor.AssignTexture();
        }

        [ContextMenu("Assign Values")]
        public void AssignValues()
        {
            Texture2D newTex = new Texture2D(28, 28);
            for (int i = 0; i < newTex.height; i++)
            {
                for (int j = 0; j < newTex.width; j++)
                {
                    newTex.SetPixel(i, j, Color.blue);
                }
            }
            newTex.Apply();
            leftPanelHorizontal.hover.background = newTex;
        }
    }

    [System.Serializable]
    public class GUIStyleStateVar
    {
        public Color backgroundColor;
        public Color textColor = Color.white;
        [SerializeField] public Texture2D backgroundTex;

        public void AssignTexture()
        {
            backgroundTex = new Texture2D(28, 28);
            for (int i = 0; i < backgroundTex.height; i++)
            {
                for (int j = 0; j < backgroundTex.width; j++)
                {
                    backgroundTex.SetPixel(i, j, backgroundColor);
                }
            }
            backgroundTex.Apply();
        }
    }
}