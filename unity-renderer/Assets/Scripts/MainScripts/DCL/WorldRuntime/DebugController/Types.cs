using System;
using Variables.SpawnPoints;

[Serializable]
internal class PreviewMenuPayload
{
    public bool enabled;
}

[Serializable]
internal class ToggleSpawnPointsPayload : SceneSpawnPointsData
{
    public string sceneId;
}