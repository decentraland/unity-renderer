using System;

internal interface ISectionUpdateSceneContributorsRequester
{
    event Action<string, SceneContributorsUpdatePayload> OnRequestUpdateSceneContributors;
}
