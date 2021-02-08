using DCL.Tutorial;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialStep_SceneObject : TutorialStep
{
    [SerializeField] AudioEvent audioEventSuccess;
    bool sceneObjectIsSelected;

    public override void OnStepStart()
    {
        base.OnStepStart();

        HUDController.i.builderInWorldMainHud.OnSceneObjectSelected += SceneObjectSelected;
    }

    public override void OnStepFinished()
    {
        base.OnStepFinished();

        HUDController.i.builderInWorldMainHud.OnSceneObjectSelected -= SceneObjectSelected;
    }


    void SceneObjectSelected(SceneObject sceneObject)
    {
        sceneObjectIsSelected = true;
    }

    public override IEnumerator OnStepExecute()
    {
        yield return new WaitUntil(() => sceneObjectIsSelected);
        audioEventSuccess.Play(true);
    }
}
