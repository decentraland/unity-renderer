using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineStarter : MonoBehaviour
{
    private static CoroutineStarter instanceValue;
    private Stack<IEnumerator> stackedCoroutines;

    public static CoroutineStarter instance
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

    private void Awake()
    {
        stackedCoroutines = new Stack<IEnumerator>();
        StartCoroutine(StackChecker());
    }

    IEnumerator StackChecker()
    {
        while (true)
        {
            if (stackedCoroutines.Count > 0)
            {
                yield return stackedCoroutines.Pop();
            }
            yield return null;
        }
    }

    public static Coroutine Start(IEnumerator function) { return instance.StartCoroutine(function); }

    public void StartStackedCoroutine(IEnumerator function)
    {
        stackedCoroutines.Push(function);
    }

    public static void Stop(Coroutine coroutine)
    {
        if (instanceValue != null && coroutine != null)
            instance.StopCoroutine(coroutine);
    }
    
    

}