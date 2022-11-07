using DCL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardsController : IBillboardsController
{
    private const int BILLBOARDS_MAX_INDEX = 1;

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
            List<Billboard> currentBillboards = new List<Billboard>(billboards);
            int indexStepLength = currentBillboards.Count / BILLBOARDS_MAX_INDEX;
            int billboardCount = 0;
            int indexCount = 0;
            yield return waitForFrameEnd;

            UpdateCameraMoved();
            bool updatePending = camUpdated;
            camUpdated = false;
            foreach (Billboard billboard in currentBillboards)
            {
                billboardCount++;
                if (indexCount < BILLBOARDS_MAX_INDEX && billboardCount >= indexStepLength)
                {
                    yield return null;
                    yield return waitForFrameEnd;
                    billboardCount = 0;
                    indexCount++;

                    UpdateCameraMoved();
                }

                if (billboard == null || billboard.Tr == null || billboard.EntityTransform == null)
                    continue;
                if (!updatePending && billboard.Tr.position == billboard.LastPosition)
                    continue;

                billboard.LastPosition = billboard.Tr.position;
                ChangeOrientation(billboard);
            }

            yield return null;
        }


        bool UpdateCameraMoved()
        {
            bool hasMoved = false;
            if (lastCamPosition != CameraPosition)
            {
                hasMoved = true;
                camUpdated = true;
            }

            lastCamPosition = CameraPosition;
            return hasMoved;
        }
    }
}