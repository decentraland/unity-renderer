using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL.Tutorial;

public class TutorialStep_PutSceneObject : TutorialStep
{
    [SerializeField] AudioEvent audioEventSuccess;
    bool sceneObjectSet = false;

    BuildModeController buildModeController;
    public override void OnStepStart()
    {
        base.OnStepStart();
        buildModeController = FindObjectOfType<BuildModeController>();
        buildModeController.OnSceneObjectPlaced += SceneObjectSelected;
    }

    public override void OnStepFinished()
    {
        base.OnStepFinished();

        buildModeController.OnSceneObjectPlaced -= SceneObjectSelected;
    }


    void SceneObjectSelected()
    {
        sceneObjectSet = true;
    }

    public override IEnumerator OnStepExecute()
    {
        yield return new WaitUntil(() => sceneObjectSet);
        audioEventSuccess.Play(true);
    }
}
