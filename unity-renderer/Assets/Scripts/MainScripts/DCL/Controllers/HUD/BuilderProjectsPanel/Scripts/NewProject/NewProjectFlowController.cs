using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using UnityEngine;

public interface INewProjectFlowController
{
    /// <summary>
    /// This will create a new project data and show the view to create it
    /// </summary>
    void NewProject();

    /// <summary>
    /// This will set the title and the description of the project
    /// </summary>
    /// <param name="title"></param>
    /// <param name="description"></param>
    void SetTitleAndDescription(string title, string description);

    /// <summary>
    /// This will set the size of the size in terms of rows and columns
    /// </summary>
    /// <param name="rows"></param>
    /// <param name="columns"></param>
    void SetRowsAndColumns(int rows, int columns);
    void Dispose();
}

public class NewProjectFlowController : INewProjectFlowController
{
    public const string VIEW_PATH = "NewProject/NewProjectFlowView";

    private ProjectData projectData;

    internal INewProjectFlowView view;

    public NewProjectFlowController()
    {
        var prefab = Resources.Load<NewProjectFlowView>(VIEW_PATH);
        view = Object.Instantiate(prefab);
    }

    public void NewProject()
    {
        projectData = new ProjectData();
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
    }

    public void Dispose() { view.Dispose(); }
}