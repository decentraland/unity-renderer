using System.Runtime.InteropServices;
using UnityEngine;

public class OpenIntercomTest : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            // OpenIntercom();
            Debug.Log("I SHOULD BE OPENING INTERCOM");
            OpenTestDesktop();
        }
    }

#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void OpenIntercom();
#else

    private static void OpenIntercom() { }

    private static void OpenTestDesktop()
    {
        string url = "https://intercom.decentraland.org";
        Application.OpenURL(url);
    }
#endif
}
