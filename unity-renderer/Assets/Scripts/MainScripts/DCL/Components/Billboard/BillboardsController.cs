using DCL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardsController : IBillboardsController
{
    private List<Billboard> billboards = new List<Billboard>();
    private Coroutine updateCoroutine;


    public static BillboardsController Create()
    {
        BillboardsController controller = new BillboardsController();
        controller.Initialize();

        return controller;
    }

    public void Initialize()
    {
        updateCoroutine = CoroutineStarter.Start(UpdateCoroutine());
    }

    public void BillboardAdded(GameObject billboardContainer)
    {
        Billboard billboard = billboardContainer.GetComponent<Billboard>();

        if (billboard != null)
            billboards.Add(billboard);
    }

    public void BillboardRemoved(GameObject billboardContainer)
    {
        Billboard billboard = billboardContainer.GetComponent<Billboard>();

        if (billboard == null || !billboards.Contains(billboard))
            return;

        billboards.Remove(billboard);
    }

    public void Dispose()
    {
        CoroutineStarter.Stop(updateCoroutine);
        billboards.Clear();
    }


    private void ChangeOrientation(Billboard billboard)
    {
        if (billboard.EntityTransform == null)
            return;

        Vector3 lookAtVector = billboard.GetLookAtVector();
        if (lookAtVector != Vector3.zero)
            billboard.EntityTransform.forward = lookAtVector;
    }


    private IEnumerator UpdateCoroutine()
    {
        WaitForEndOfFrame waitForFrameEnd = new WaitForEndOfFrame();

        while (true)
        {
            yield return waitForFrameEnd;

            foreach (Billboard billboard in billboards)
            {
                if (billboard.EntityTransform == null)
                    continue;
                if (billboard.Tr.position == billboard.LastPosition)
                    continue;

                billboard.LastPosition = billboard.Tr.position;

                ChangeOrientation(billboard);
            }

            yield return null;
        }
    }
}