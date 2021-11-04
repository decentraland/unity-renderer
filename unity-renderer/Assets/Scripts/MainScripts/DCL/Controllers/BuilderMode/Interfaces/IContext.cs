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

        IBuilderMainPanelController panelHUD { get; }

        IEditorContext editorContext { get; }

        IInitialSceneReferences sceneReferences { get; }

        void Dispose();
    }

    public interface IEditorContext
    {
        public BIWGodModeDynamicVariables godModeDynamicVariablesAsset { get; }
        public BIWFirstPersonDynamicVariables firstPersonDynamicVariablesAsset { get; }

        IBuilderEditorHUDController editorHUD { get;  }

        // BIW Controllers
        IBIWOutlinerController outlinerController { get;  }
        IBIWInputHandler inputHandler { get;  }
        IBIWInputWrapper inputWrapper { get;  }
        IBIWPublishController publishController { get;  }
        IBIWCreatorController creatorController { get;  }
        IBIWModeController modeController { get;  }
        IBIWFloorHandler floorHandler { get; }
        IBIWEntityHandler entityHandler { get;  }
        IBIWActionController actionController { get;  }
        IBIWSaveController saveController { get;  }
        IBIWRaycastController raycastController { get;  }
        IBIWGizmosController gizmosController { get;  }

        IInitialSceneReferences sceneReferences { get; }

        void Dispose();
    }
}