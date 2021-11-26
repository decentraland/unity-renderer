using System;

internal interface ISectionUpdateSceneDataRequester
{
    event Action<string, SceneDataUpdatePayload> OnRequestUpdateSceneData;
}