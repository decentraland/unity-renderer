using UnityEngine;

namespace DCL.Skybox
{
    public static class TextureLayerFunctionality
    {
        public static void ApplyTextureLayer(Material selectedMat, float dayTime, float normalizedDayTime, int slotNum, TextureLayer layer, float cycleTime = 24, bool changeAlllValues = true)
        {
            float endTimeEdited = layer.timeSpan_End;
            float dayTimeEdited = dayTime;
            if (layer.timeSpan_End < layer.timeSpan_start)
            {
                endTimeEdited = cycleTime + layer.timeSpan_End;
                if (dayTime < layer.timeSpan_start)
                {
                    dayTimeEdited = cycleTime + dayTime;
                }
            }
            float normalizedLayerTime = Mathf.InverseLerp(layer.timeSpan_start, endTimeEdited, dayTimeEdited);

            selectedMat.SetFloat(SkyboxShaderUtils.GetLayerProperty("_layerType_", slotNum), (int)layer.layerType);

            bool fadeInChange = CheckFadingIn(selectedMat, dayTime, normalizedDayTime, slotNum, layer, cycleTime);

            bool fadeOutChange = CheckFadingOut(selectedMat, dayTime, normalizedDayTime, slotNum, layer, cycleTime);

            if (!fadeInChange && !fadeOutChange)
            {
                selectedMat.SetFloat(SkyboxShaderUtils.GetLayerProperty("_fadeTime_", slotNum), 1);
            }

            switch (layer.layerType)
            {
                case LayerType.Planar:
                case LayerType.Radial:
                    ApplyPlanarTextureLayer(selectedMat, dayTime, normalizedLayerTime, slotNum, layer, true);
                    break;
                case LayerType.Satellite:
                    ApplySatelliteTextureLayer(selectedMat, dayTime, normalizedLayerTime, slotNum, layer, true);
                    break;
                case LayerType.Cubemap:
                    ApplyCubemapTextureLayer(selectedMat, dayTime, normalizedLayerTime, slotNum, layer, true);
                    break;
                case LayerType.Particles:
                    ApplyParticleTextureLayer(selectedMat, dayTime, normalizedLayerTime, slotNum, layer, true);
                    break;
                default:
                    break;
            }
        }

        private static bool CheckFadingIn(Material selectedMat, float dayTime, float normalizedDayTime, int slotNum, TextureLayer layer, float cycleTime = 24, bool changeAlllValues = true)
        {
            bool fadeChanged = false;
            float fadeInCompletionTime = layer.timeSpan_start + layer.fadingInTime;
            float dayTimeEdited = dayTime;
            if (dayTime < layer.timeSpan_start)
            {
                dayTimeEdited = 24 + dayTime;
            }

            if (dayTimeEdited < fadeInCompletionTime)
            {
                float percentage = Mathf.InverseLerp(layer.timeSpan_start, fadeInCompletionTime, dayTimeEdited);
                fadeChanged = true;
                selectedMat.SetFloat(SkyboxShaderUtils.GetLayerProperty("_fadeTime_", slotNum), percentage);
            }

            return fadeChanged;
        }

        private static bool CheckFadingOut(Material selectedMat, float dayTime, float normalizedDayTime, int slotNum, TextureLayer layer, float cycleTime = 24, bool changeAlllValues = true)
        {
            bool fadeChanged = false;
            float endTimeEdited = layer.timeSpan_End;
            float dayTimeEdited = dayTime;

            if (layer.timeSpan_End < layer.timeSpan_start)
            {
                endTimeEdited = cycleTime + layer.timeSpan_End;
            }

            if (dayTime < layer.timeSpan_start)
            {
                dayTimeEdited = cycleTime + dayTime;
            }


            float fadeOutStartTime = endTimeEdited - layer.fadingOutTime;

            if (dayTimeEdited > fadeOutStartTime)
            {
                float percentage = Mathf.InverseLerp(endTimeEdited, fadeOutStartTime, dayTimeEdited);
                fadeChanged = true;
                selectedMat.SetFloat(SkyboxShaderUtils.GetLayerProperty("_fadeTime_", slotNum), percentage);
            }

            return fadeChanged;
        }

        private static void ApplyCubemapTextureLayer(Material selectedMat, float dayTime, float normalizedLayerTime, int layerNum, TextureLayer layer, bool changeAlllValues = true)
        {
            if (changeAlllValues)
            {
                selectedMat.SetFloat(SkyboxShaderUtils.GetLayerProperty("_RenderDistance_", layerNum), 0);
                selectedMat.SetTexture(SkyboxShaderUtils.GetLayerProperty("_tex_", layerNum), null);
                selectedMat.SetTexture(SkyboxShaderUtils.GetLayerProperty("_cubemap_", layerNum), layer.cubemap);
                selectedMat.SetTexture(SkyboxShaderUtils.GetLayerProperty("_normals_", layerNum), null);
                selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_timeFrame_", layerNum), new Vector4(layer.timeSpan_start, layer.timeSpan_End));
                selectedMat.SetFloat(SkyboxShaderUtils.GetLayerProperty("_lightIntensity_", layerNum), layer.tintPercentage / 100);
                selectedMat.SetFloat(SkyboxShaderUtils.GetLayerProperty("_normalIntensity_", layerNum), 0);
                selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_distortIntAndSize_", layerNum), Vector2.zero);
                selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_distortSpeedAndSharp_", layerNum), Vector4.zero);
                // Particles
                selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_rowAndCollumns_", layerNum), Vector2.zero);
                selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_particlesMainParameters_", layerNum), Vector4.zero);
                selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_particlesSecondaryParameters_", layerNum), Vector4.zero);
            }


            selectedMat.SetColor(SkyboxShaderUtils.GetLayerProperty("_color_", layerNum), layer.color.Evaluate(normalizedLayerTime));

            // Set cubemap rotation. (Shader variable reused)
            if (layer.movementTypeCubemap == MovementType.PointBased)
            {
                Vector3 currentRotation = TransitioningValues.GetTransitionValue(layer.rotations_Vector3, normalizedLayerTime * 100);
                selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_tilingAndOffset_", layerNum), new Vector4(currentRotation.x, currentRotation.y, currentRotation.z, 0));
                selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_speedAndRotation_", layerNum), new Vector4(0, 0, 0, 0));
            }
            else
            {
                selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_tilingAndOffset_", layerNum), new Vector4(0, 0, 0, 0));
                selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_speedAndRotation_", layerNum), new Vector4(layer.speed_Vector3.x, layer.speed_Vector3.y, layer.speed_Vector3.z, 0));
            }

        }

        private static void ApplyPlanarTextureLayer(Material selectedMat, float dayTime, float normalizedLayerTime, int layerNum, TextureLayer layer, bool changeAlllValues = true)
        {
            selectedMat.SetFloat(SkyboxShaderUtils.GetLayerProperty("_RenderDistance_", layerNum), TransitioningValues.GetTransitionValue(layer.renderDistance, normalizedLayerTime * 100, 3.4f));
            selectedMat.SetTexture(SkyboxShaderUtils.GetLayerProperty("_tex_", layerNum), layer.texture);
            selectedMat.SetTexture(SkyboxShaderUtils.GetLayerProperty("_cubemap_", layerNum), null);

            selectedMat.SetColor(SkyboxShaderUtils.GetLayerProperty("_color_", layerNum), layer.color.Evaluate(normalizedLayerTime));


            if (layer.movementTypePlanar_Radial == MovementType.Speed)
            {
                // speed and Rotation
                float rot = 0;
                if (layer.layerType == LayerType.Planar)
                {
                    rot = TransitioningValues.GetTransitionValue(layer.rotations_float, normalizedLayerTime * 100);
                }

                selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_speedAndRotation_", layerNum), new Vector4(layer.speed_Vector2.x, layer.speed_Vector2.y, rot));

                // Tiling and Offset
                Vector4 t = new Vector4(layer.tiling.x, layer.tiling.y, 0, 0);
                selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_tilingAndOffset_", layerNum), t);
            }
            else
            {
                // speed and Rotation
                float rot = 0;
                if (layer.layerType == LayerType.Planar)
                {
                    rot = TransitioningValues.GetTransitionValue(layer.rotations_float, normalizedLayerTime * 100);
                }

                selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_speedAndRotation_", layerNum), new Vector4(0, 0, rot));

                // Tiling and Offset
                Vector2 currentOffset = TransitioningValues.GetTransitionValue(layer.offset, normalizedLayerTime * 100);
                Vector4 t = new Vector4(layer.tiling.x, layer.tiling.y, currentOffset.x, currentOffset.y);
                selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_tilingAndOffset_", layerNum), t);
            }


            // Time frame
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_timeFrame_", layerNum), new Vector4(layer.timeSpan_start, layer.timeSpan_End));
            // Tint
            selectedMat.SetFloat(SkyboxShaderUtils.GetLayerProperty("_lightIntensity_", layerNum), layer.tintPercentage / 100);

            // Reset Particle related Params
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_rowAndCollumns_", layerNum), layer.flipbookRowsAndColumns);
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_particlesMainParameters_", layerNum), new Vector4(layer.flipbookAnimSpeed, 0, 0, 0));
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_particlesSecondaryParameters_", layerNum), Vector4.zero);

            // Apply Distortion Values
            Vector2 distortIntAndSize = new Vector2(TransitioningValues.GetTransitionValue(layer.distortIntensity, normalizedLayerTime * 100), TransitioningValues.GetTransitionValue(layer.distortSize, normalizedLayerTime * 100));
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_distortIntAndSize_", layerNum), distortIntAndSize);

            Vector2 distortSpeed = TransitioningValues.GetTransitionValue(layer.distortSpeed, normalizedLayerTime * 100);
            Vector2 distortSharpness = TransitioningValues.GetTransitionValue(layer.distortSharpness, normalizedLayerTime * 100);
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_distortSpeedAndSharp_", layerNum), new Vector4(distortSpeed.x, distortSpeed.y, distortSharpness.x, distortSharpness.y));
        }

        private static void ApplySatelliteTextureLayer(Material selectedMat, float dayTime, float normalizedLayerTime, int layerNum, TextureLayer layer, bool changeAlllValues = true)
        {
            selectedMat.SetTexture(SkyboxShaderUtils.GetLayerProperty("_tex_", layerNum), layer.texture);
            selectedMat.SetTexture(SkyboxShaderUtils.GetLayerProperty("_cubemap_", layerNum), null);

            selectedMat.SetColor(SkyboxShaderUtils.GetLayerProperty("_color_", layerNum), layer.color.Evaluate(normalizedLayerTime));

            if (layer.movementTypeSatellite == MovementType.Speed)
            {
                // Tiling and Offset
                Vector2 currentWidthHeight = TransitioningValues.GetTransitionValue(layer.satelliteWidthHeight, normalizedLayerTime * 100, new Vector2(1, 1));
                Vector4 t = new Vector4(currentWidthHeight.x, currentWidthHeight.y, 0, 0);
                selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_tilingAndOffset_", layerNum), t);


                // speed and Rotation
                float rot = TransitioningValues.GetTransitionValue(layer.rotations_float, normalizedLayerTime * 100);
                selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_speedAndRotation_", layerNum), new Vector4(layer.speed_Vector2.x, layer.speed_Vector2.y, rot));
            }
            else
            {
                // Tiling and Offset
                Vector2 currentOffset = TransitioningValues.GetTransitionValue(layer.offset, normalizedLayerTime * 100);
                Vector2 currentWidthHeight = TransitioningValues.GetTransitionValue(layer.satelliteWidthHeight, normalizedLayerTime * 100, new Vector2(1, 1));
                Vector4 t = new Vector4(currentWidthHeight.x, currentWidthHeight.y, currentOffset.x, currentOffset.y);
                selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_tilingAndOffset_", layerNum), t);

                // speed and Rotation
                float rot = TransitioningValues.GetTransitionValue(layer.rotations_float, normalizedLayerTime * 100);
                selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_speedAndRotation_", layerNum), new Vector4(0, 0, rot));
            }

            // Time frame
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_timeFrame_", layerNum), new Vector4(layer.timeSpan_start, layer.timeSpan_End));
            // Tint
            selectedMat.SetFloat(SkyboxShaderUtils.GetLayerProperty("_lightIntensity_", layerNum), layer.tintPercentage / 100);

            // Reset Particle related Params
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_rowAndCollumns_", layerNum), layer.flipbookRowsAndColumns);
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_particlesMainParameters_", layerNum), new Vector4(layer.flipbookAnimSpeed, 0, 0, 0));
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_particlesSecondaryParameters_", layerNum), Vector4.zero);

            // Reset Distortion values
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_distortIntAndSize_", layerNum), Vector2.zero);
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_distortSpeedAndSharp_", layerNum), Vector4.zero);

        }

        private static void ApplyParticleTextureLayer(Material selectedMat, float dayTime, float normalizedLayerTime, int layerNum, TextureLayer layer, bool changeAlllValues = true)
        {
            // Reset Unused params
            selectedMat.SetFloat(SkyboxShaderUtils.GetLayerProperty("_RenderDistance_", layerNum), 0);
            selectedMat.SetTexture(SkyboxShaderUtils.GetLayerProperty("_cubemap_", layerNum), null);
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_distortIntAndSize_", layerNum), Vector2.zero);
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_distortSpeedAndSharp_", layerNum), Vector4.zero);


            // Time frame
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_timeFrame_", layerNum), new Vector4(layer.timeSpan_start, layer.timeSpan_End));
            // Tint
            selectedMat.SetFloat(SkyboxShaderUtils.GetLayerProperty("_lightIntensity_", layerNum), layer.tintPercentage / 100);

            // Particles
            selectedMat.SetTexture(SkyboxShaderUtils.GetLayerProperty("_tex_", layerNum), layer.texture);
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_rowAndCollumns_", layerNum), layer.flipbookRowsAndColumns);
            selectedMat.SetColor(SkyboxShaderUtils.GetLayerProperty("_color_", layerNum), layer.color.Evaluate(normalizedLayerTime));
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_tilingAndOffset_", layerNum), new Vector4(layer.particleTiling.x, layer.particleTiling.y, layer.particlesOffset.x, layer.particlesOffset.y));
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_speedAndRotation_", layerNum), TransitioningValues.GetTransitionValue(layer.particleRotation, normalizedLayerTime * 100));
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_particlesMainParameters_", layerNum), new Vector4(layer.flipbookAnimSpeed, layer.particlesAmount, layer.particleMinSize, layer.particleMaxSize));
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_particlesSecondaryParameters_", layerNum), new Vector4(layer.particlesHorizontalSpread, layer.particlesVerticalSpread, layer.particleMinFade, layer.particleMaxFade));


        }

    }
}
