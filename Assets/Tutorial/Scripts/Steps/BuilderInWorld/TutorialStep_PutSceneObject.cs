using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL.Tutorial;

public class TutorialStep_PutSceneObject : TutorialStep
{
    [SerializeField] AudioEvent audioEventSuccess;
    bool sceneObjectSet = false;

    BuilderInWorldController builderInWorldController;
    public override void OnStepStart()
    {
        base.OnStepStart();
        builderInWorldController = FindObjectOfType<BuilderInWorldController>();
        builderInWorldController.OnSceneObjectPlaced += SceneObjectSelected;
    }

    public override void OnStepFinished()
    {
        base.OnStepFinished();

        builderInWorldController.OnSceneObjectPlaced -= SceneObjectSelected;
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
