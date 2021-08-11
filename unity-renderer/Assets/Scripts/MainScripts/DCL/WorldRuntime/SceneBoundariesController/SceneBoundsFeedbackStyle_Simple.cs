﻿using DCL.Models;
using UnityEngine;
using System.Collections.Generic;

namespace DCL.Controllers
{
    public class SceneBoundsFeedbackStyle_Simple : ISceneBoundsFeedbackStyle
    {
        private readonly List<Renderer> disabledRenderers = new List<Renderer>();

        public void ApplyFeedback(MeshesInfo meshesInfo, bool isInsideBoundaries)
        {
            if (meshesInfo?.renderers == null || meshesInfo.renderers.Length == 0)
                return;

            if (meshesInfo?.currentShape != null && !meshesInfo.currentShape.IsVisible())
                return;

            for (int i = 0; i < meshesInfo.renderers.Length; i++)
            {
                if (meshesInfo.renderers[i] == null)
                    continue;

                if (isInsideBoundaries == meshesInfo.renderers[i].enabled)
                    continue;

                meshesInfo.renderers[i].enabled = isInsideBoundaries;

                if (isInsideBoundaries && disabledRenderers.Contains(meshesInfo.renderers[i]))
                    disabledRenderers.Remove( meshesInfo.renderers[i]);
                else if (!isInsideBoundaries && !disabledRenderers.Contains(meshesInfo.renderers[i]))
                    disabledRenderers.Add( meshesInfo.renderers[i]);
            }
        }

        public void CleanFeedback()
        {
            foreach (var renderer in disabledRenderers)
            {
                renderer.enabled = true;
            }

            disabledRenderers.Clear();
        }

        public List<Material> GetOriginalMaterials(MeshesInfo meshesInfo)
        {
            List<Material> result = new List<Material>();

            for (int i = 0; i < meshesInfo.renderers.Length; i++)
            {
                result.AddRange(meshesInfo.renderers[i].sharedMaterials);
            }

            return result;
        }
    }
}