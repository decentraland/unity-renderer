using System.Collections;
using System.Collections.Generic;
using DCL;

public enum IsolatedMode
{
    BUILDER = 0
}

public class IsolatedConfig
{
    public IsolatedMode mode;
    public object payload;
}

public class IsolatedBuilderConfig
{
    public string sceneId;
    public bool recreateScene;
    public bool killPortableExperiences;
    public ILand land;
}

public class ILand
{
    public string sceneId;
    public string baseUrl;
    public string baseUrlBundles;

    public MappingsResponse mappingsResponse;
    public SceneJsonData sceneJsonData;
}

public class SceneJsonData
{
    public string main;
    public SceneParcels scene;
}

public class SceneParcels
{
    public string @base;
    public string[] parcels;
}

public class MappingsResponse
{
    public string parcel_id;
    public string root_cid;

    public List<ContentServerUtils.MappingPair> contents;
}