using System.Collections.Generic;

internal interface IProjectsListener
{
    void OnSetProjects(Dictionary<string, IProjectCardView> projects);
    void OnProjectAdded(IProjectCardView project);
    void OnProjectRemoved(IProjectCardView project);
}