using System.Collections;
using System.Collections.Generic;
using DCL;

public class IsolatedConfig
{
    public string sceneId;
    public bool recreateScene;
    public ILand land;
}

public class ILand
{
    public string sceneId;
    public object sceneJsonData;
    public string baseUrl;
    public string baseUrlBundles;

    public MappingsResponse mappingsResponse;
}

public class MappingsResponse
{
    public string parcel_id;
    public string root_cid;

    public List<ContentServerUtils.MappingPair> contents;
}