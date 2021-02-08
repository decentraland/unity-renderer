using UnityEngine;

public class WebServerComponent : MonoBehaviour
{
    static SimpleHTTPServer _server;
    string wwwPath;
    const int PORT = 9991;

    void Awake()
    {
        if (_server == null)
        {
            var appPath = Application.dataPath;
            wwwPath = appPath + "/TestWebserverRoot";
            _server = new SimpleHTTPServer(wwwPath, PORT);
            Debug.Log("Starting web server... (" + wwwPath + ")");
        }
    }

    public void Restart()
    {
        if (_server == null)
        {
            Awake();
        }
        else
        {
            _server.Restart(wwwPath, PORT);
        }
    }

    void OnDestroy()
    {
        if (_server == null)
        {
            _server.Stop();
        }
    }
}