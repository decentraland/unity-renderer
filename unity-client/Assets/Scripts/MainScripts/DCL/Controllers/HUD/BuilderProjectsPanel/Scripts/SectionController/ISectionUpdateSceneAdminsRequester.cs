using System;

internal interface ISectionUpdateSceneAdminsRequester
{
    event Action<string, SceneAdminsUpdatePayload> OnRequestUpdateSceneAdmins;
}