using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using UnityEngine;

namespace DCL.Builder
{
    public class BuilderScene : IBuilderScene
    {
        public Manifest.Manifest manifest { get; set; }
        public Texture2D sceneScreenshotTexture { get; set; }
        public IParcelScene scene { get; set; }
    }

}