using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public class SkyboxEditorStyles
    {
        private static SkyboxEditorStyles instance;
        public static SkyboxEditorStyles Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SkyboxEditorStyles();
                }
                return instance;
            }
        }
        public GUIStyle foldoutStyle;
        public GUIStyle renderingMarkerStyle;
        public GUIStyle configurationStyle;
        public GUIStyle percentagePartStyle;
        public GUIStyle transitioningBoxStyle;

        public SkyboxEditorStyles()
        {
            if (foldoutStyle == null)
            {
                foldoutStyle = new GUIStyle(EditorStyles.foldout);
                foldoutStyle.fixedWidth = 2;
            }

            if (renderingMarkerStyle == null)
            {
                renderingMarkerStyle = new GUIStyle(EditorStyles.label);
                renderingMarkerStyle.fontSize = 18;
            }

            if (configurationStyle == null)
            {
                configurationStyle = new GUIStyle();
                configurationStyle.alignment = TextAnchor.MiddleCenter;
                configurationStyle.margin = new RectOffset(150, 200, 0, 0);
            }

            if (percentagePartStyle == null)
            {
                percentagePartStyle = new GUIStyle();
                percentagePartStyle.alignment = TextAnchor.MiddleCenter;
            }

            if (transitioningBoxStyle == null)
            {
                transitioningBoxStyle = new GUIStyle("box");
            }
        }
    }
}