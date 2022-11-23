using DCL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardsController : IBillboardsController
{
    private readonly List<Billboard> billboards = new List<Billboard>();
    private Coroutine updateCoroutine;

    private Vector3Variable CameraPosition => CommonScriptableObjects.cameraPosition;
    private Vector3 lastCamPosition;
    private bool camUpdated;


    public static BillboardsController Create()
    {
        BillboardsController controller = new BillboardsController();

        return controller;
    }

    public void Initialize()
    {
        updateCoroutine = CoroutineStarter.Start(UpdateCoroutine());
    }

    public void BillboardAdded(GameObject billboardContainer)
    {
        if (billboardContainer.TryGetComponent<Billboard>(out var billboard))
        {
            billboards.Add(billboard);
            ChangeOrientation(billboard);
        }
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

        Vector3 lookAtVector = billboard.GetLookAtVector(CameraPosition);
        if (lookAtVector != Vector3.zero)
            billboard.EntityTransform.forward = lookAtVector;
    }


    private IEnumerator UpdateCoroutine()
    {
        WaitForEndOfFrame waitForFrameEnd = new WaitForEndOfFrame();

        while (true)
        {
            Debug.Log(Time.realtimeSinceStartup);
            yield return waitForFrameEnd;

            camUpdated = lastCamPosition != CameraPosition;
            lastCamPosition = CameraPosition;
            foreach (Billboard billboard in billboards)
            {
                if (billboard.Tr == null || billboard.EntityTransform == null)
                    continue;
                if (!camUpdated && billboard.Tr.position == billboard.LastPosition)
                    continue;

                billboard.LastPosition = billboard.Tr.position;
                ChangeOrientation(billboard);
            }

            yield return null;
        }
    }
}