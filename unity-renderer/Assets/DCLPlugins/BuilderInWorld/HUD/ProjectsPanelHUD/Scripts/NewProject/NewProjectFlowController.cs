using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Builder;
using DCL.Configuration;
using UnityEngine;
using Object = UnityEngine.Object;

public interface INewProjectFlowController
{
    /// <summary>
    /// When a new project is created
    /// </summary>
    event Action<ProjectData> OnNewProjectCrated;

    /// <summary>
    /// This will create a new project data and show the view to create it
    /// </summary>
    void NewProject();

    /// <summary>
    /// Hide the view
    /// </summary>
    void Hide();

    void Dispose();

    /// <summary>
    /// This will return true if the new project windows is active
    /// </summary>
    /// <returns></returns>
    bool IsActive();
}

public class NewProjectFlowController : INewProjectFlowController
{
    public const string VIEW_PATH = "NewProject/NewProjectFlowView";

    public event Action<ProjectData> OnNewProjectCrated;

    internal ProjectData projectData;

    internal INewProjectFlowView view;

    public NewProjectFlowController()
    {
        var prefab = Resources.Load<NewProjectFlowView>(VIEW_PATH);
        var instantiateView = Object.Instantiate(prefab);
        Initilizate(instantiateView);
    }

    public NewProjectFlowController(INewProjectFlowView view) { Initilizate(view); }

    private void Initilizate(INewProjectFlowView view)
    {
        this.view = view;
        view.OnTittleAndDescriptionSet += SetTitleAndDescription;
        view.OnSizeSet += SetRowsAndColumns;
        view.OnViewHide += SetCreatingProjectStatusToFalse;
    }

    public void Hide()
    {
        view.Hide();
        DataStore.i.builderInWorld.areShortcutsBlocked.Set(false);
    }

    public void SetCreatingProjectStatusToFalse()
    {
        DataStore.i.builderInWorld.areShortcutsBlocked.Set(false);
    }
    
    public void Dispose()
    {
        view.OnTittleAndDescriptionSet -= SetTitleAndDescription;
        view.OnSizeSet -= SetRowsAndColumns;
        view.OnViewHide -= SetCreatingProjectStatusToFalse;
        view.Dispose();
    }

    public bool IsActive() { return view.IsActive(); }

    public void NewProject()
    {
        UserProfile userProfile = UserProfile.GetOwnUserProfile();
        if (userProfile.isGuest)
        {
            BIWUtils.ShowGenericNotification(BIWSettings.GUEST_CANT_USE_BUILDER);
            return;
        }

        projectData = new ProjectData();
        projectData.id = Guid.NewGuid().ToString();
        projectData.eth_address = UserProfile.GetOwnUserProfile().ethAddress;
        DataStore.i.builderInWorld.areShortcutsBlocked.Set(true);
        view.ShowNewProjectTitleAndDescrition();
    }

    public void SetTitleAndDescription(string title, string description)
    {
        projectData.title = title;
        projectData.description = description;
    }

    public void SetRowsAndColumns(int columns, int rows)
    {
        projectData.rows = rows;
        projectData.cols = columns;

        NewProjectCreated();
    }

    internal void NewProjectCreated()
    {
        projectData.created_at = DateTime.UtcNow;
        projectData.updated_at = DateTime.UtcNow;
        DataStore.i.builderInWorld.areShortcutsBlocked.Set(false);
        view.Reset();
        OnNewProjectCrated?.Invoke(projectData);
        view.Hide();
    }
}