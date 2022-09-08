using UnityEngine;

public interface IHomeLocationController
{
    void SetHomeScene(string location);
    void SetHomeScene(Vector2 location);
}