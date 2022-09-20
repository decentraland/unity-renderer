using UnityEngine;
using System.Collections;

public class SimpleSpin : MonoBehaviour
{
    [Range(-100, 100)] public int speed = 0;

    void Update()
    {
        Spin(speed);
    }

    private void Spin(float speed)
    {
        transform.Rotate(new UnityEngine.Vector3(0, speed * Time.deltaTime, 0));
    }
}