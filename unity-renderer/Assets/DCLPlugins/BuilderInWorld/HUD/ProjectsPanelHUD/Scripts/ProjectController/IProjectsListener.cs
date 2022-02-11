using System;
using System.Collections.Generic;
using DCL.Builder;

/// <summary>
/// This is a listener that can set/add/remove projects cards from the consumer
/// </summary>
internal interface IProjectsListener
{
    /// <summary>
    /// This will set the projects dictionary indexed by ID
    /// </summary>
    /// <param name="projects">Dictionary of projects with ID indexed</param>
    void OnSetProjects(Dictionary<string, IProjectCardView> projects);

    /// <summary>
    /// This will add a project card to the consumer
    /// </summary>
    /// <param name="project">Project card to add</param>
    void OnProjectAdded(IProjectCardView project);

    /// <summary>
    /// This will remove a project card from the consumer
    /// </summary>
    /// <param name="project">Project card to remove</param>
    void OnProjectRemoved(IProjectCardView project);
}