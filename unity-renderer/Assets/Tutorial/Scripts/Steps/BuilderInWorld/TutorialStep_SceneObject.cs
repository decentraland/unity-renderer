using DCL.Tutorial;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

[ExcludeFromCodeCoverage]
public class TutorialStep_SceneObject : TutorialStep
{
    [SerializeField] AudioEvent audioEventSuccess;
    bool catalogItemIsSelected;

    private Context context;

    public void SetContext(Context context) { this.context = context; }

    public override void OnStepStart()
    {
        base.OnStepStart();

        context.editorContext.editorHUD.OnCatalogItemSelected += SceneObjectSelected;
    }

    public override void OnStepFinished()
    {
        base.OnStepFinished();

        context.editorContext.editorHUD.OnCatalogItemSelected -= SceneObjectSelected;
    }

    void SceneObjectSelected(CatalogItem sceneObject) { catalogItemIsSelected = true; }

    public override IEnumerator OnStepExecute()
    {
        yield return new WaitUntil(() => catalogItemIsSelected);
        audioEventSuccess.Play(true);
    }
}