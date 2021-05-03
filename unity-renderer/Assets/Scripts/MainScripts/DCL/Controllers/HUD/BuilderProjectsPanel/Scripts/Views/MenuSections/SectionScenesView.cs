using System;
using UnityEngine;
using UnityEngine.UI;

public class SectionScenesView : MonoBehaviour, IDisposable
{
    [Header("Cards Containers")]
    [SerializeField] internal Transform deployedSceneContainer;
    [SerializeField] internal Transform projectSceneContainer;

    [Header("Screens")]
    [SerializeField] internal GameObject emptyScreen;
    [SerializeField] internal GameObject contentScreen;

    [Header("Group Containers")]
    [SerializeField] internal GameObject inWorldContainer;
    [SerializeField] internal GameObject projectsContainer;

    [Header("Buttons")]
    [SerializeField] internal Button btnInWorldViewAll;
    [SerializeField] internal Button btnProjectsViewAll;

    private bool isDestroyed = false;

    private void OnDestroy()
    {
        isDestroyed = true;
    }

    public void Dispose()
    {
        if (!isDestroyed)
        {
            Destroy(gameObject);
        }
    }
}
