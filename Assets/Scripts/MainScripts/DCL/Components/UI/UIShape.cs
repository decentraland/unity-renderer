using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCL.Helpers;
using DCL.Controllers;
using DCL.Models;

namespace DCL.Components
{
    public class UIShape : BaseDisposable
    {
        [System.Serializable]
        public class Model
        {
            public string id;
            public string parentComponent;
            public bool visible = true;
        }

        public override string componentName => "UIShape";
        public RectTransform transform;

        public UIShape(ParcelScene scene) : base(scene)
        {
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            return null;
        }

        public override void Dispose()
        {
            Utils.SafeDestroy(transform.gameObject);

            base.Dispose();
        }
    }
}