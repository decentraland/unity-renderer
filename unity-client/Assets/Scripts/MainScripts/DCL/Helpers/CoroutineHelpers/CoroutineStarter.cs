using System.Collections;
using UnityEngine;

public class CoroutineStarter : MonoBehaviour
{
    private static CoroutineStarter instanceValue;

    private static CoroutineStarter instance
    {
        get
        {
            if (instanceValue == null)
            {
                instanceValue = new GameObject("_CoroutineStarter").AddComponent<CoroutineStarter>();
            }

            return instanceValue;
        }
    }

    public static Coroutine Start(IEnumerator function)
    {
        return instance.StartCoroutine(function);
    }

    public static void Stop(Coroutine coroutine)
    {
        if (instanceValue != null && coroutine != null)
            instance.StopCoroutine(coroutine);
    }

}
