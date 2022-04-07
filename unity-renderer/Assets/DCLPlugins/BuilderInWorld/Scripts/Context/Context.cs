using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;

namespace DCL.Builder
{
    public class Context : IContext
    {
        private const string PROJECT_REFERENCES_PATH = "ScriptableObjects/ProjectReferences";
        private const string INPUTS_PATH = "ScriptableObjects/InputReferences";

        //Scriptable Objects
        public BIWProjectReferences projectReferencesAsset { get;  set; }
        public BIWInputsReferences inputsReferencesAsset { get;  set; }

        //Builder Parts
        public IBuilderMainPanelController panelHUD { get; internal set; }
        public IBIWEditor editor  { get; internal set; }
        public IBuilderAPIController builderAPIController { get; internal set; }
        public ISceneManager sceneManager  { get; internal set; }
        public ICameraController cameraController  { get; internal set; }
        public ISceneReferences sceneReferences { get; internal set; }
        public IPublisher publisher { get; internal set; }
        public ICommonHUD commonHUD { get; internal set; }

        //Editor
        public IEditorContext editorContext { get; internal set; }

        public Context(IBIWEditor editor,
            IBuilderMainPanelController panelHUD,
            IBuilderAPIController builderAPIController,
            ISceneManager sceneManager,
            ICameraController cameraController,
            IPublisher publisher,
            ICommonHUD commonHUD,
            IBuilderEditorHUDController editorHUD,
            IBIWOutlinerController outlinerController,
            IBIWInputHandler inputHandler,
            IBIWInputWrapper inputWrapper,
            IBIWPublishController publishController,
            IBIWCreatorController creatorController,
            IBIWModeController modeController,
            IBIWFloorHandler floorHandler,
            IBIWEntityHandler entityHandler,
            IBIWActionController actionController,
            IBIWSaveController saveController,
            IBIWRaycastController raycastController,
            IBIWGizmosController gizmosController,
            ISceneReferences sceneReferences)
        {
            projectReferencesAsset = Resources.Load<BIWProjectReferences>(PROJECT_REFERENCES_PATH);
            inputsReferencesAsset = Resources.Load<BIWInputsReferences>(INPUTS_PATH);

            this.sceneReferences = sceneReferences;

            //Builder parts
            this.panelHUD = panelHUD;
            this.editor = editor;
            this.builderAPIController = builderAPIController;
            this.sceneManager = sceneManager;
            this.cameraController = cameraController;
            this.publisher = publisher;
            this.commonHUD = commonHUD;

            editorContext = new EditorContext(editorHUD,
                outlinerController,
                inputHandler,
                inputWrapper,
                publishController,
                creatorController,
                modeController,
                floorHandler,
                entityHandler,
                actionController,
                saveController,
                raycastController,
                gizmosController,
                sceneReferences
            );
        }

        public void Dispose()
        {
            editorContext.Dispose();
            panelHUD.Dispose();
            projectReferencesAsset = null;
            inputsReferencesAsset = null;
        }
    }

    public class EditorContext : IEditorContext
    {
        private const string GOD_MODE_DYNAMIC_VARIABLE_PATH = "ScriptableObjects/GodModeVariables";
        private const string FIRST_PERSON_DYNAMIC_VARIABLE_PATH = "ScriptableObjects/FirstPersonVariables";

        BIWGodModeDynamicVariables IEditorContext.godModeDynamicVariablesAsset => godModeDynamicVariablesAssetReference;
        BIWFirstPersonDynamicVariables IEditorContext.firstPersonDynamicVariablesAsset => firstPersonDynamicVariablesAssetReference;

        //HUD
        public IBuilderEditorHUDController editorHUD { get; private set; }

        // BIW Controllers
        public IBIWOutlinerController outlinerController { get; private set; }
        public IBIWInputHandler inputHandler { get; private set; }
        public IBIWInputWrapper inputWrapper { get; private set; }
        public IBIWPublishController publishController { get; private set; }
        public IBIWCreatorController creatorController { get; private set; }
        public IBIWModeController modeController { get; private set; }
        public IBIWFloorHandler floorHandler => floorHandlerReference;

        internal IBIWFloorHandler floorHandlerReference;
        public IBIWEntityHandler entityHandler { get; private set; }
        public IBIWActionController actionController { get; private set; }
        public IBIWSaveController saveController { get; private set; }
        public IBIWRaycastController raycastController { get; private set; }
        public IBIWGizmosController gizmosController { get; private set; }
        public ISceneReferences sceneReferences { get; private set; }

        //Scriptable Objects
        internal BIWGodModeDynamicVariables godModeDynamicVariablesAssetReference;
        internal BIWFirstPersonDynamicVariables firstPersonDynamicVariablesAssetReference;

        public EditorContext(IBuilderEditorHUDController editorHUD,
            IBIWOutlinerController outlinerController,
            IBIWInputHandler inputHandler,
            IBIWInputWrapper inputWrapper,
            IBIWPublishController publishController,
            IBIWCreatorController creatorController,
            IBIWModeController modeController,
            IBIWFloorHandler floorHandler,
            IBIWEntityHandler entityHandler,
            IBIWActionController actionController,
            IBIWSaveController saveController,
            IBIWRaycastController raycastController,
            IBIWGizmosController gizmosController,
            ISceneReferences sceneReferences)
        {
            godModeDynamicVariablesAssetReference = Resources.Load<BIWGodModeDynamicVariables>(GOD_MODE_DYNAMIC_VARIABLE_PATH);
            firstPersonDynamicVariablesAssetReference = Resources.Load<BIWFirstPersonDynamicVariables>(FIRST_PERSON_DYNAMIC_VARIABLE_PATH);

            this.editorHUD = editorHUD;

            this.outlinerController = outlinerController;
            this.inputHandler = inputHandler;
            this.inputWrapper = inputWrapper;
            this.publishController = publishController;
            this.creatorController = creatorController;
            this.modeController = modeController;
            this.floorHandlerReference = floorHandler;
            this.entityHandler = entityHandler;
            this.actionController = actionController;
            this.saveController = saveController;
            this.raycastController = raycastController;
            this.gizmosController = gizmosController;
            this.sceneReferences = sceneReferences;
        }

        public void Dispose()
        {
            editorHUD.Dispose();
            outlinerController.Dispose();
            inputHandler.Dispose();
            inputWrapper.Dispose();
            publishController.Dispose();
            creatorController.Dispose();
            modeController.Dispose();
            floorHandler.Dispose();
            entityHandler.Dispose();
            actionController.Dispose();
            saveController.Dispose();
            raycastController.Dispose();
            gizmosController.Dispose();

            sceneReferences.Dispose();


            godModeDynamicVariablesAssetReference = null;
            firstPersonDynamicVariablesAssetReference = null;
        }
    }
}