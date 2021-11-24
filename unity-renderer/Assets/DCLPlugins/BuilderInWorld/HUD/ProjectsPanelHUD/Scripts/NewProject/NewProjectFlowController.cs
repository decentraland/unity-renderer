using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
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
    
    public NewProjectFlowController(INewProjectFlowView view)
    {
        Initilizate(view);
    }

    private void Initilizate(INewProjectFlowView view)
    {
        this.view = view;
        view.OnTittleAndDescriptionSet += SetTitleAndDescription;
        view.OnSizeSet += SetRowsAndColumns;
    }

    public void Hide() { view.Hide(); }
    
    public void Dispose()
    {
        view.OnTittleAndDescriptionSet -= SetTitleAndDescription;
        view.OnSizeSet -= SetRowsAndColumns;
        view.Dispose();
    }
    
    public bool IsActive()
    {
        return view.IsActive();
    }

    public void NewProject()
    {
        projectData = new ProjectData();
        projectData.id = Guid.NewGuid().ToString();
        projectData.eth_address = UserProfile.GetOwnUserProfile().ethAddress;
        
        view.ShowNewProjectTitleAndDescrition();
    }

    public void SetTitleAndDescription(string title, string description)
    {
        projectData.title = title;
        projectData.description = description;
    }

    public void SetRowsAndColumns(int rows, int columns)
    {
        projectData.rows = rows;
        projectData.cols = columns;
        
        NewProjectCreated();
    }

    internal void NewProjectCreated()
    {
        projectData.created_at = DateTime.UtcNow;
        projectData.updated_at = DateTime.UtcNow;

        view.Reset();
        OnNewProjectCrated?.Invoke(projectData);
        view.Hide();
    }
}