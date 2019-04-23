using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;
using UnityGLTF;

namespace DCL.Components
{
    public class GLTFShape : BaseLoadableShape<GLTFLoader>
    {
        public GLTFShape(ParcelScene scene) : base(scene)
        {
        }
    }
}
