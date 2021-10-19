using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using UnityEngine;

public interface INewProjectFlowController
{
    void NewProject();
    void SetTitleAndDescription(string title, string description);
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