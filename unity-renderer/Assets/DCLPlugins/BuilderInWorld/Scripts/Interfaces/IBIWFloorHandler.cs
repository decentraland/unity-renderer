using System;
using System.Collections.Generic;
using UnityEngine;

public interface IBIWFloorHandler : IBIWController
{
    void CreateDefaultFloor();
    void CreateFloor(CatalogItem floorSceneObject);
    bool IsCatalogItemFloor(CatalogItem floorSceneObject);
    void ChangeFloor(CatalogItem newFloorObject);
    event Action OnAllParcelsFloorLoaded;
    Dictionary<long, GameObject> floorPlaceHolderDict { get; set; }
    void CleanUp();
}