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
        public IBuilderScene.SceneType sceneType { get; }

        private bool isNew = false;

        public BuilderScene(Manifest.IManifest manifest, IBuilderScene.SceneType sceneType, bool isNew = false)
        {
            this.isNew = isNew;
            this.manifest = manifest;

            this.sceneType = sceneType;
        }

        public void UpdateManifestFromScene() { manifest.scene = ManifestTranslator.ParcelSceneToWebBuilderScene((ParcelScene)scene); }

        public void SetScene(IParcelScene scene) { this.scene = scene; }
        public bool HasBeenCreatedThisSession() { return isNew; }
    }

}