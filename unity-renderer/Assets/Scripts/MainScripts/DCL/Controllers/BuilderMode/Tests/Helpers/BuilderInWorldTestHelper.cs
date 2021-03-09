using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class BuilderInWorldTestHelper 
{
    public static void CreateTestCatalogLocal()
    {
        AssetCatalogBridge.ClearCatalog();

        string jsonPath = Utils.GetTestsAssetsPath() + "/SceneObjects/catalog.json";


        if(File.Exists(jsonPath))
        {
            Debug.Log("File found!");
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(jsonPath, FileMode.Open);

            string jsonValue = formatter.Deserialize(stream) as string;
            AssetCatalogBridge.i.AddFullSceneObjectCatalog(jsonValue);
            stream.Close();
        }
    }
}
