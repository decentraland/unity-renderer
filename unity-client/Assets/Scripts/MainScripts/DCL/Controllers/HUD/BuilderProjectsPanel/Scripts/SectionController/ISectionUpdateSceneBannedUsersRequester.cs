using System;

internal interface ISectionUpdateSceneBannedUsersRequester
{
    event Action<string, SceneBannedUsersUpdatePayload> OnRequestUpdateSceneBannedUsers;
}