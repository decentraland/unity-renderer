using DCL.Components;
using DCL.Helpers;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UISizeFitter : MonoBehaviour
{
    public static bool VERBOSE = false;
    public bool adjustWidth = true;
    public bool adjustHeight = true;

    RectTransform canvasChildHookRT;

    public void Refresh()
    {
        FitSizeToChildren(adjustWidth, adjustHeight);
    }

    void EnsureCanvasChildHookRectTransform()
    {
        if (canvasChildHookRT == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas)
                canvasChildHookRT = canvas.GetComponent<RectTransform>();
            else
                canvasChildHookRT = GetComponentInParent<RectTransform>();
        }
    }

    public void FitSizeToChildren(bool adjustWidth = true, bool adjustHeight = true)
    {
        UIReferencesContainer[] containers = GetComponentsInChildren<UIReferencesContainer>();

        EnsureCanvasChildHookRectTransform();

        RectTransform rt = transform as RectTransform;

        if (rt == null || canvasChildHookRT == null || containers == null || containers.Length == 0)
        {
            return;
        }

        Rect finalRect = new Rect();
        finalRect.xMax = float.MinValue;
        finalRect.yMax = float.MinValue;
        finalRect.xMin = float.MaxValue;
        finalRect.yMin = float.MaxValue;

        Vector3[] corners = new Vector3[4];

        Vector3 center = Vector3.zero;

        Transform[] children = transform.Cast<Transform>().ToArray();

        for (int i = 0; i < children.Length; i++)
        {
            //NOTE(Brian): We need to remove the children because we are going to try and resize the father without altering
            //             their positions--we are going to re-add them later. Note that i'm setting the parent to canvas instead null,
            //             this is because worldPositionStays parameter isn't working with rotated/scaled canvases, if I reparent to the
            //             current canvas, this is solved.
            children[i].SetParent(canvasChildHookRT, true);
        }

        foreach (UIReferencesContainer rc in containers)
        {
            RectTransform r = rc.childHookRectTransform;

            if (r == transform)
            {
                continue;
            }

            if (VERBOSE)
            {
                Debug.Log($"..... container... {r.name} ... w = {r.sizeDelta.x} h = {r.sizeDelta.y}", rc.gameObject);
            }

            r.GetWorldCorners(corners);

            //NOTE(Brian): We want the coords in canvas space to solve CanvasScaler issues and world canvas arbitrary transform values.
            corners[0] = canvasChildHookRT.InverseTransformPoint(corners[0]);
            corners[1] = canvasChildHookRT.InverseTransformPoint(corners[1]);
            corners[2] = canvasChildHookRT.InverseTransformPoint(corners[2]);
            corners[3] = canvasChildHookRT.InverseTransformPoint(corners[3]);

            //TODO(Brian): This'll look cleaner with a Bounds.EncapsulateBounds solution.
            for (int i = 0; i < corners.Length; i++)
            {
                if (corners[i].x < finalRect.xMin)
                {
                    finalRect.xMin = corners[i].x;
                }

                if (corners[i].x > finalRect.xMax)
                {
                    finalRect.xMax = corners[i].x;
                }

                if (corners[i].y < finalRect.yMin)
                {
                    finalRect.yMin = corners[i].y;
                }

                if (corners[i].y > finalRect.yMax)
                {
                    finalRect.yMax = corners[i].y;
                }
            }
        }

        if (!adjustWidth)
        {
            finalRect.width = rt.sizeDelta.x;
        }

        if (!adjustHeight)
        {
            finalRect.height = rt.sizeDelta.y;
        }

        if (VERBOSE)
        {
            Debug.Log($".....end! final rect... w = {finalRect.width} h = {finalRect.height}");
        }

        //NOTE(Brian): In this last step, we need to transform from canvas space to world space.
        //             We need to use "position" because assumes its pivot in the lower right corner of the rect in world space.
        //             This is exactly what we are looking for as we want an anchor agnostic solution.
        rt.position = canvasChildHookRT.TransformPoint(finalRect.min + (finalRect.size * rt.pivot));
        rt.sizeDelta = new Vector2(finalRect.width, finalRect.height);

        for (int i = 0; i < children.Length; i++)
        {
            children[i].SetParent(transform, true);
        }
    }

    void RefreshRecursively_Node(UISizeFitter fitter)
    {
        fitter.Refresh();

        LayoutGroup p = fitter.GetComponentInParent<LayoutGroup>();

        if (p != null)
        {
            Utils.ForceRebuildLayoutImmediate(p.transform as RectTransform);
        }
    }

    public void RefreshRecursively(Transform startTransform = null)
    {
        Utils.InverseTransformChildTraversal<UISizeFitter>(RefreshRecursively_Node, transform);
    }

#if UNITY_EDITOR
    private void Awake()
    {
        forceRefresh = false;
    }

    //NOTE(Brian): Only used for debugging
    public bool forceRefresh = false;

    public void Update()
    {
        if (forceRefresh)
        {
            RefreshRecursively();
            forceRefresh = false;
        }
    }
#endif
}