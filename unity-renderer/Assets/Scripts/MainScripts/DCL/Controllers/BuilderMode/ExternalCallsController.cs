
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ExternalCallsController
{
    private static ExternalCallsController instanceValue;

    List<IEnumerator> callList = new List<IEnumerator>();

    Coroutine callerCourutine;
    bool isRunningCalls;
    public static ExternalCallsController i
    {
        get
        {
            if (instanceValue == null)
            {
                instanceValue = new ExternalCallsController();
            }

            return instanceValue;
        }
    }

    public void GetContentAsString(string url, Action<string> functionToCall)
    {
        CoroutineStarter.Start(MakeGetCall(url,null, functionToCall));

    }

    public void GetContentAsByteArray(string url, Action<string,byte[]> functionToCall)
    {
        callList.Add(MakeGetCall(url, functionToCall, null));
        if(!isRunningCalls)  
                callerCourutine = CoroutineStarter.Start(MakeExternalCalls());
    }

    IEnumerator MakeExternalCalls()
    {
        isRunningCalls = true;
        while (callList.Count > 0)
        {
            IEnumerator nextCall = callList[0];
            callList.Remove(nextCall);
            yield return CoroutineStarter.Start(nextCall);
        }
        isRunningCalls = false;
    }
 
    static IEnumerator MakeGetCall(string url, Action<string,byte[]> byteArrayFunctionToCall,Action<string> functionToCall)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        UnityWebRequestAsyncOperation www2 = www.SendWebRequest();

        while (!www2.isDone)
        {
            yield return null;
        }
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {         
            byte[] byteArray = www.downloadHandler.data;
            if(byteArrayFunctionToCall != null)
            {
                byteArrayFunctionToCall.Invoke(url,byteArray);
            }
            else if (functionToCall != null)
            {
                string result = System.Text.Encoding.UTF8.GetString(byteArray);
                functionToCall?.Invoke(result);
            }
           
        }
    }
}
