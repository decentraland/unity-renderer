using DCL.Models;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DCL.Rendering
{
    public class RendererDebugGUI : MonoBehaviour
    {
        private const int FIELD_HEIGHT = 30;
        private const int LABEL_SIZE = 500;

        [SerializeField] private Rect scrollArea;
        [SerializeField] private Rect viewRectArea;

        private Vector2 scroll = Vector2.zero;
        private GUIStyle labelStyle;

        private void Update()
        {
            if (Input.GetKey(KeyCode.N))
                scroll.y += 500 * Time.deltaTime;
            else if (Input.GetKey(KeyCode.J))
                scroll.y -= 500 * Time.deltaTime;
        }

        private void OnGUI()
        {
            if (DataStore.i == null) return;
            if (DataStore.i.sceneWorldObjects.sceneData == null) return;

            labelStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 8,
            };

            Vector3 playerPosition = CommonScriptableObjects.playerUnityPosition;
            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

            var yPos = 0;
            const int POS_X = 0;

            scroll = GUI.BeginScrollView(scrollArea, scroll, viewRectArea);

            foreach (KeyValuePair<int, DataStore_WorldObjects.SceneData> entry in DataStore.i.sceneWorldObjects.sceneData)
            {
                foreach (Renderer renderer in entry.Value.renderers.Get())
                {
                    if (renderer == null)
                        continue;

                    Bounds bounds = MeshesInfoUtils.GetSafeBounds(renderer.bounds, renderer.transform.position);
                    Vector3 boundingPoint = bounds.ClosestPoint(playerPosition);
                    float distance = Vector3.Distance(playerPosition, boundingPoint);
                    float boundsSize = bounds.size.magnitude;
                    float viewportSize = boundsSize / distance * Mathf.Rad2Deg;
                    float shadowTexelSize = CullingControllerUtils.ComputeShadowMapTexelSize(boundsSize, urp.shadowDistance, urp.mainLightShadowmapResolution);
                    bool isEmissive = CullingControllerUtils.IsEmissive(renderer);
                    bool isOpaque = CullingControllerUtils.IsOpaque(renderer);
                    Material material = renderer.sharedMaterial;

                    DrawLabelAsList(POS_X, ref yPos, $"name: {renderer.name}");
                    DrawLabelAsList(POS_X, ref yPos, $"active: {renderer.gameObject.activeSelf}");
                    DrawLabelAsList(POS_X, ref yPos, $"enabled: {renderer.enabled}");
                    DrawLabelAsList(POS_X, ref yPos, $"isVisible: {renderer.isVisible}");
                    DrawLabelAsList(POS_X, ref yPos, $"forceRenderingOff: {renderer.forceRenderingOff}");
                    DrawLabelAsList(POS_X, ref yPos, $"rendererPriority: {renderer.rendererPriority}");
                    DrawLabelAsList(POS_X, ref yPos, $"sortingOrder: {renderer.sortingOrder}");
                    DrawLabelAsList(POS_X, ref yPos, $"isPartOfStaticBatch: {renderer.isPartOfStaticBatch}");
                    DrawLabelAsList(POS_X, ref yPos, $"layer: {renderer.gameObject.layer}");
                    DrawLabelAsList(POS_X, ref yPos, $"material: {material.name}, {material.shader.name}");
                    DrawLabelAsList(POS_X, ref yPos, $"receiveShadows: {renderer.receiveShadows}");
                    DrawLabelAsList(POS_X, ref yPos, $"shadowCastingMode: {renderer.shadowCastingMode}");
                    DrawLabelAsList(POS_X, ref yPos, $"staticShadowCaster: {renderer.staticShadowCaster}");
                    DrawLabelAsList(POS_X, ref yPos, $"viewportSize: {viewportSize}");
                    DrawLabelAsList(POS_X, ref yPos, $"shadowTexelSize: {shadowTexelSize}");
                    DrawLabelAsList(POS_X, ref yPos, $"isEmissive: {isEmissive}");
                    DrawLabelAsList(POS_X, ref yPos, $"isOpaque: {isOpaque}");
                    DrawLabelAsList(POS_X, ref yPos, "-----------------------------");
                }
            }

            GUI.EndScrollView();
        }

        private void DrawLabelAsList(int xPos, ref int yPos, string label)
        {
            GUI.Label(new Rect(Width(xPos + FIELD_HEIGHT), Height(FIELD_HEIGHT + yPos), Width(LABEL_SIZE), Height(FIELD_HEIGHT)),
                label,
                labelStyle);
            yPos += FIELD_HEIGHT + 2;
        }

        private float Width(float value) =>
            value * Screen.width / 1920f;

        private float Height(float value) =>
            value * Screen.height / 1080f;
    }
}
