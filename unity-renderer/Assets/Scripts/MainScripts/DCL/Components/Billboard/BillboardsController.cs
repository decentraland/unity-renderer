using DCL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardsController : IBillboardsController
{
    private readonly List<IBillboard> billboards = new List<IBillboard>();
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

    public void BillboardAdded(IBillboard billboard)
    {
        billboards.Add(billboard);
        ChangeOrientation(billboard);
    }

    public void BillboardRemoved(IBillboard billboard)
    {
        if (!billboards.Contains(billboard))
            return;

        billboards.Remove(billboard);
    }

    public void Dispose()
    {
        CoroutineStarter.Stop(updateCoroutine);
        billboards.Clear();
    }


    private void ChangeOrientation(IBillboard billboard)
    {
        if (billboard.EntityTransform == null)
            return;

        Vector3 lookAtVector = billboard.GetLookAtVector(CameraPosition);
        if (lookAtVector != Vector3.zero)
            billboard.EntityTransform.forward = lookAtVector;
    }


    private IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            camUpdated = lastCamPosition != CameraPosition;
            lastCamPosition = CameraPosition;
            for (int i = billboards.Count - 1; i >= 0; i--)
            {
                IBillboard billboard = billboards[i];
                if (billboard == null)
                {
                    billboards.RemoveAt(i);
                    continue;
                }
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