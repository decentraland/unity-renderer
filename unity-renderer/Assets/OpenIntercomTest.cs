using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenIntercomTest : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            OpenIntercom();
            Debug.Log("I SHOULD BE OPENING INTERCOM");
        }
    }

#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static void OpenIntercom();
#else


    private static void OpenIntercom() { }
#endif
}
