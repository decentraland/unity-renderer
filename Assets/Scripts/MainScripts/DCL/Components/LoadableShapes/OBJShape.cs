using DCL.Controllers;
using DCL.Helpers;
using System.Collections;
using UnityEngine;

namespace DCL.Components
{
    public class OBJShape : BaseLoadableShape<OBJLoader>
    {
        public OBJShape(ParcelScene scene) : base(scene)
        {
        }
    }
}
