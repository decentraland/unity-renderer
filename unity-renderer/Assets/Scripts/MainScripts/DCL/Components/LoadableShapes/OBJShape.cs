using System;
using DCL.Controllers;

namespace DCL.Components
{
    public class OBJShape : LoadableShape<LoadWrapper_OBJ, LoadableShape.Model>
    {
        public OBJShape(IParcelScene scene) : base(scene)
        {
        }

        public override void CallWhenReady(Action<BaseDisposable> callback)
        {
            callback.Invoke(this);
        }
    }
}