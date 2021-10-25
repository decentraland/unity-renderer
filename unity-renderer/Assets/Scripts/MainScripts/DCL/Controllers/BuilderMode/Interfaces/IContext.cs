using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Builder
{
    public interface IContext
    {
        //Scriptable Objects
        BIWProjectReferences projectReferencesAsset { get; }
        BIWInputsReferences inputsReferencesAsset { get; }

        IEditorContext editorContext { get; }
    }

    public interface IEditorContext
    {
        IBuilderEditorHUDController editorHUD { get;  }

        // BIW Controllers
        IBIWOutlinerController outlinerController { get;  }
        IBIWInputHandler inputHandler { get;  }
        IBIWInputWrapper inputWrapper { get;  }
        IBIWPublishController publishController { get;  }
        IBIWCreatorController creatorController { get;  }
        IBIWModeController modeController { get;  }
        IBIWFloorHandler floorHandler { get; internal set; }
        IBIWEntityHandler entityHandler { get;  }
        IBIWActionController actionController { get;  }
        IBIWSaveController saveController { get;  }
        IBIWRaycastController raycastController { get;  }
        IBIWGizmosController gizmosController { get;  }

        IInitialSceneReferences.Data sceneReferences { get; }
    }
}