using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using UnityEngine;
using Object = UnityEngine.Object;

public interface INewProjectFlowController
{
    event Action<ProjectData> OnNewProjectCrated;
    /// <summary>
    /// This will create a new project data and show the view to create it
    /// </summary>
    void NewProject();
    
    void Dispose();
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
        view = Object.Instantiate(prefab);

        view.OnTittleAndDescriptionSet += SetTitleAndDescription;
        view.OnSizeSet += SetRowsAndColumns;
    }
    
    public void Dispose()
    {
        view.OnTittleAndDescriptionSet -= SetTitleAndDescription;
        view.OnSizeSet -= SetRowsAndColumns;
        view.Dispose();
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
        projectData.colums = columns;
        
        NewProjectCreated();
    }

    internal void NewProjectCreated()
    {
        view.Reset();
        OnNewProjectCrated?.Invoke(projectData);
        view.Hide();
    }
}