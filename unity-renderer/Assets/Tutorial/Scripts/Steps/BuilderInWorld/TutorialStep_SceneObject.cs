using DCL.Tutorial;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialStep_SceneObject : TutorialStep
{
    [SerializeField] AudioEvent audioEventSuccess;
    bool catalogItemIsSelected;

    public override void OnStepStart()
    {
        base.OnStepStart();

        HUDController.i.builderInWorldMainHud.OnCatalogItemSelected += SceneObjectSelected;
    }

    public override void OnStepFinished()
    {
        base.OnStepFinished();

        HUDController.i.builderInWorldMainHud.OnCatalogItemSelected -= SceneObjectSelected;
    }

    void SceneObjectSelected(CatalogItem sceneObject)
    {
        catalogItemIsSelected = true;
    }

    public override IEnumerator OnStepExecute()
    {
        yield return new WaitUntil(() => catalogItemIsSelected);
        audioEventSuccess.Play(true);
    }
}
