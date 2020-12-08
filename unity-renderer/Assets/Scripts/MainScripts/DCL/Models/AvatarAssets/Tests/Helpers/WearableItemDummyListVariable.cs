using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DCL.Helpers;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "WearableItemList", menuName = "Variables/Tests/WearableItemList")]
public class WearableItemDummyListVariable : ScriptableObject
{
    public List<WearableItemDummy> list;

    [ContextMenu("Load test catalog")]
    private void FillWithTestCatalog()
    {
        string file = "TestCatalog.json";
        var catalogJson = File.ReadAllText(Utils.GetTestAssetsPathRaw() + $"/Avatar/{file}"); //Utils.GetTestAssetPath returns an URI not compatible with the really convenient File.ReadAllText
        list = Newtonsoft.Json.JsonConvert.DeserializeObject<WearableItemDummy[]>(catalogJson).ToList(); // JsonUtility cannot deserialize jsons whose root is an array
    }
}
