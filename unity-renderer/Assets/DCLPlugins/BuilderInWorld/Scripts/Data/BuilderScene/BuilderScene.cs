using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using UnityEngine;

namespace DCL.Builder
{
    public class BuilderScene : IBuilderScene
    {
        public Manifest.IManifest manifest { get; internal set; }
        public Texture2D sceneScreenshotTexture { get; set; }
        public Texture2D aerialScreenshotTexture { get; set; }

        public IParcelScene scene { get; internal set; }
        public IBuilderScene.SceneVersion sceneVersion { get; internal set; }
        public IBuilderScene.SceneType sceneType { get; }
        
        public Vector2Int landCoordsAsociated { get; set; }

        private bool isNew = false;

        public BuilderScene(Manifest.IManifest manifest, IBuilderScene.SceneType sceneType, bool isNew = false)
        {
            this.isNew = isNew;
            this.manifest = manifest;

            this.sceneType = sceneType;

            sceneVersion = IBuilderScene.SceneVersion.LEGACY;
        }

        public void UpdateManifestFromScene()
        {
            manifest.scene = ManifestTranslator.ParcelSceneToWebBuilderScene((ParcelScene)scene);
            manifest.project.updated_at = DateTime.UtcNow;
            manifest.project.scene_id = manifest.scene.id;
        }

        public void SetScene(IParcelScene scene) { this.scene = scene; }
        
        public bool HasBeenCreatedThisSession() { return isNew; }
    }

}