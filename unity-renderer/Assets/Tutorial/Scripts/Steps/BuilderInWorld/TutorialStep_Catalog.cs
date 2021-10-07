using DCL.Tutorial;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

[ExcludeFromCodeCoverage]
public class TutorialStep_Catalog : TutorialStep
{
    public GameObject arrowGO;
    [SerializeField] AudioEvent audioEventSuccess;
    bool isCatalogOpen = false;

    private Context context;
    public void SetContext(Context context) { this.context = context; }

    public override void OnStepStart()
    {
        base.OnStepStart();

        context.editorContext.editorHUD.OnCatalogOpen += CatalogOpened;
    }

    public override void OnStepFinished()
    {
        base.OnStepFinished();

        context.editorContext.editorHUD.OnCatalogOpen -= CatalogOpened;
    }

    void CatalogOpened()
    {
        isCatalogOpen = true;
        arrowGO.SetActive(false);
    }

    public override IEnumerator OnStepExecute()
    {
        yield return new WaitUntil(() => isCatalogOpen);
        audioEventSuccess.Play(true);
    }

}