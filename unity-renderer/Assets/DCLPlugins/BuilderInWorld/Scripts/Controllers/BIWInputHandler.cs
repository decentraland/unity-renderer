using DCL.Controllers;
using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DCL.Builder;

public class BIWInputHandler : BIWController, IBIWInputHandler
{
    private const float MS_BETWEEN_INPUT_INTERACTION = 200;

    private IBIWActionController actionController;
    private IBIWModeController modeController;
    private IBIWInputWrapper inputWrapper;
    private IBIWOutlinerController outlinerController;
    private IBIWEntityHandler entityHandler;

    private InputAction_Trigger toggleRedoActionInputAction;
    private InputAction_Trigger toggleUndoActionInputAction;
    private InputAction_Hold multiSelectionInputAction;

    private InputAction_Hold.Started multiSelectionStartDelegate;
    private InputAction_Hold.Finished multiSelectionFinishedDelegate;

    private InputAction_Trigger.Triggered redoDelegate;
    private InputAction_Trigger.Triggered undoDelegate;

    private bool isMultiSelectionActive = false;

    private float nexTimeToReceiveInput;

    public override void Initialize(IContext context)
    {
        base.Initialize(context);

        actionController = context.editorContext.actionController;
        modeController = context.editorContext.modeController;
        inputWrapper = context.editorContext.inputWrapper;
        outlinerController = context.editorContext.outlinerController;
        entityHandler = context.editorContext.entityHandler;

        toggleRedoActionInputAction = context.inputsReferencesAsset.toggleRedoActionInputAction;
        toggleUndoActionInputAction = context.inputsReferencesAsset.toggleUndoActionInputAction;
        multiSelectionInputAction = context.inputsReferencesAsset.multiSelectionInputAction;

        if ( context.editorContext.editorHUD != null)
        {
            context.editorContext.editorHUD.OnStopInput += StopInput;
            context.editorContext.editorHUD.OnResumeInput += ResumeInput;
        }

        redoDelegate = (action) => RedoAction();
        undoDelegate = (action) => UndoAction();

        toggleRedoActionInputAction.OnTriggered += redoDelegate;
        toggleUndoActionInputAction.OnTriggered += undoDelegate;

        multiSelectionStartDelegate = (action) => StartMultiSelection();
        multiSelectionFinishedDelegate = (action) => EndMultiSelection();

        BIWInputWrapper.OnMouseClick += MouseClick;
        BIWInputWrapper.OnMouseClickOnUI += MouseClickOnUI;
        modeController.OnInputDone += InputDone;

        multiSelectionInputAction.OnStarted += multiSelectionStartDelegate;
        multiSelectionInputAction.OnFinished += multiSelectionFinishedDelegate;
    }

    public override void Dispose()
    {
        base.Dispose();

        toggleRedoActionInputAction.OnTriggered -= redoDelegate;
        toggleUndoActionInputAction.OnTriggered -= undoDelegate;

        multiSelectionInputAction.OnStarted -= multiSelectionStartDelegate;
        multiSelectionInputAction.OnFinished -= multiSelectionFinishedDelegate;

        BIWInputWrapper.OnMouseClick -= MouseClick;
        BIWInputWrapper.OnMouseClickOnUI -= MouseClickOnUI;
        modeController.OnInputDone -= InputDone;
        if ( context.editorContext.editorHUD != null)
        {
            context.editorContext.editorHUD.OnStopInput -= StopInput;
            context.editorContext.editorHUD.OnResumeInput -= ResumeInput;
        }
    }

    public override void Update()
    {
        base.Update();

        if (Time.timeSinceLevelLoad < nexTimeToReceiveInput)
            return;

        if (Utils.IsCursorLocked || modeController.IsGodModeActive())
            CheckEditModeInput();
        modeController.CheckInput();
    }

    private void CheckEditModeInput()
    {
        outlinerController.CheckOutline();

        if (entityHandler.IsAnyEntitySelected())
        {
            modeController.CheckInputSelectedEntities();
        }
    }

    private void StartMultiSelection()
    {
        isMultiSelectionActive = true;
        entityHandler.SetMultiSelectionActive(isMultiSelectionActive);
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        modeController.StartMultiSelection();
    }

    private void EndMultiSelection()
    {
        isMultiSelectionActive = false;
        entityHandler.SetMultiSelectionActive(isMultiSelectionActive);
        modeController.EndMultiSelection();
        outlinerController.CancelUnselectedOutlines();
    }

    private void MouseClick(int buttonID, Vector3 position)
    {
        if (!isEditModeActive)
            return;

        if (Time.timeSinceLevelLoad < nexTimeToReceiveInput)
            return;

        if (!BIWUtils.IsPointerOverUIElement())
            context.editorContext.editorHUD.HideExtraBtns();

        if (Utils.IsCursorLocked || modeController.IsGodModeActive())
        {
            if (buttonID == 0)
            {
                MouseClickDetected();
                InputDone();
                return;
            }

            outlinerController.CheckOutline();
        }
    }

    private void MouseClickOnUI(int buttonID, Vector3 position) {  context.editorContext.editorHUD.HideExtraBtns(); }

    public bool IsMultiSelectionActive() => isMultiSelectionActive;

    private void RedoAction()
    {
        actionController.TryToRedoAction();
        InputDone();
    }

    private void UndoAction()
    {
        InputDone();

        if (modeController.ShouldCancelUndoAction())
            return;

        actionController.TryToUndoAction();
    }

    private void MouseClickDetected() { modeController.MouseClickDetected(); }

    private void InputDone() { nexTimeToReceiveInput = Time.timeSinceLevelLoad + MS_BETWEEN_INPUT_INTERACTION / 1000; }

    private void StopInput() { inputWrapper.StopInput(); }

    private void ResumeInput() { inputWrapper.ResumeInput(); }
}