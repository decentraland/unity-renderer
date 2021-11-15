using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class MouseCatcherBridge : MonoBehaviour
    {
        //Externally -ONLY- called by the browser
        public void UnlockCursorBrowser(int val) { SceneReferences.i.mouseCatcher.UnlockCursorBrowser(val); }
    }
}