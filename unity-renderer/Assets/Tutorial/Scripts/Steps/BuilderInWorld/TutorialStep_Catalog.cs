using DCL.Tutorial;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialStep_Catalog : TutorialStep
{
    public GameObject arrowGO;
    [SerializeField] AudioEvent audioEventSuccess;
    bool isCatalogOpen = false;


    public override void OnStepStart()
    {
        base.OnStepStart();

        HUDController.i.buildModeHud.OnCatalogOpen += CatalogOpened;
    }

    public override void OnStepFinished()
    {
        base.OnStepFinished();

        HUDController.i.buildModeHud.OnCatalogOpen -= CatalogOpened;
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
