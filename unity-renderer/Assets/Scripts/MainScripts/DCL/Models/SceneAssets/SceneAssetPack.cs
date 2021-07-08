using DCL.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneAssetPack
{
    public string id;
    public string title;
    public string thumbnail;
    public string user_id;
    public string created_at;
    public string updated_at;
    public string eth_address;
    public List<SceneObject> assets;

    public string ComposeThumbnailUrl() { return BIWUrlUtils.GetUrlAssetPackContent() + thumbnail; }
}