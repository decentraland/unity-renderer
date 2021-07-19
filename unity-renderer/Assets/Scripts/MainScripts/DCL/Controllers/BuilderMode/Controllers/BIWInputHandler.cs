using DCL.Controllers;
using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IBIWInputHandler { }

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

    public override void Init(BIWContext biwContext)
    {
        base.Init(biwContext);

        actionController = biwContext.actionController;
        modeController = biwContext.modeController;
        inputWrapper = biwContext.inputWrapper;
        outlinerController = biwContext.outlinerController;
        entityHandler = biwContext.entityHandler;

        toggleRedoActionInputAction = biwContext.inputsReferences.toggleRedoActionInputAction;
        toggleUndoActionInputAction = biwContext.inputsReferences.toggleUndoActionInputAction;
        multiSelectionInputAction = biwContext.inputsReferences.multiSelectionInputAction;

        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.OnStopInput += StopInput;
            HUDController.i.builderInWorldMainHud.OnResumeInput += ResumeInput;
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
        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.OnStopInput -= StopInput;
            HUDController.i.builderInWorldMainHud.OnResumeInput -= ResumeInput;
        }
    }

    public override void Update()
    {
        base.Update();

        if (Time.timeSinceLevelLoad < nexTimeToReceiveInput)
            return;

        if (Utils.isCursorLocked || modeController.IsGodModeActive())
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
            HUDController.i.builderInWorldMainHud.HideExtraBtns();

        if (Utils.isCursorLocked || modeController.IsGodModeActive())
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

    private void MouseClickOnUI(int buttonID, Vector3 position) { HUDController.i.builderInWorldMainHud.HideExtraBtns(); }

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