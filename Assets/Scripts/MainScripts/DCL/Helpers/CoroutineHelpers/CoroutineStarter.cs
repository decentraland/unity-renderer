using System.Collections;
using UnityEngine;

public class CoroutineStarter : MonoBehaviour
{
    private static CoroutineStarter i;

    private static CoroutineStarter instance
    {
        get
        {
            if (i == null)
            {
                i = new GameObject("_CoroutineStarter").AddComponent<CoroutineStarter>();
            }

            return i;
        }
    }

    public static Coroutine Start(IEnumerator function)
    {
        return instance.StartCoroutine(function);
    }

    public static void Stop(Coroutine coroutine)
    {
        instance.StopCoroutine(coroutine);
    }

}
