using System;
using System.Collections.Generic;

public interface IBuilderProjectsPanelBridge
{
    event Action<string> OnProjectsSet;

    void OnReceivedProjects(string payload);
    void SendFetchProjects();
    void SendDuplicateProject(string id);
    void SendDownload(string id);
    void SendShare(string id);
    void SendUnPublish(string id);
    void SendDelete(string id);
    void SendQuitContributor(string id);
    void SendSceneDataUpdate(string id, SceneDataUpdatePayload payload);
    void SendSceneContributorsUpdate(string id, SceneContributorsUpdatePayload payload);
    void SendSceneAdminsUpdate(string id, SceneAdminsUpdatePayload payload);
    void SendSceneBannedUsersUpdate(string id, SceneBannedUsersUpdatePayload payload);
}
