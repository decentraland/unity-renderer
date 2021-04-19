using System;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.UI;

internal class SectionLandView : MonoBehaviour, IDisposable
{
    [SerializeField] public LandElementView landElementView;
    [SerializeField] public ScrollRect scrollRect;
    [SerializeField] public GameObject contentContainer;
    [SerializeField] public GameObject emptyContainer;
    [SerializeField] public Button buttonNoLandCTA;

    private bool isDestroyed = false;

    private void OnDestroy()
    {
        isDestroyed = true;
    }

    public void SetParent(Transform parent)
    {
        transform.SetParent(parent);
        transform.ResetLocalTRS();
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void ResetScrollRect()
    {
        scrollRect.verticalNormalizedPosition = 1;
    }

    public Transform GetLandElementsContainer()
    {
        return landElementView.GetParent();
    }

    public LandElementView GetLandElementeBaseView()
    {
        return landElementView;
    }

    public void SetEmpty(bool isEmpty)
    {
        emptyContainer.SetActive(isEmpty);
        contentContainer.SetActive(!isEmpty);
    }
    
    public void Dispose()
    {
        if (!isDestroyed)
        {
            Destroy(gameObject);
        }
    }
}
