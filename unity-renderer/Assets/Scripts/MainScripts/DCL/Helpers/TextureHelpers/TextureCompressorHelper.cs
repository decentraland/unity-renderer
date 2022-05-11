using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureCompressorHelper
{
    private static readonly List<object> _compressionList = new List<object>();

    public static IEnumerator ThrottledCompress(object context, Texture2D texture, bool uploadToGPU, Action<Texture2D> OnSuccess, Action<Exception> OnFail, bool generateMimpaps = false, bool linear = false)
    {
        _compressionList.Add(context);

        yield return new DCL.WaitUntil( () => _compressionList[0] == context );

        yield return  TextureHelpers.ThrottledCompress(texture, uploadToGPU, result =>
        {
            _compressionList.Remove(context);
            OnSuccess(result);
        }, exception =>
        {
            _compressionList.Remove(context);
            OnFail(exception);
        }, generateMimpaps, linear);
    }

    public static void CancelCompression(object context)
    {
        Debug.Log("Cancelled! " + context);
        _compressionList.Remove(context);
    }

}
