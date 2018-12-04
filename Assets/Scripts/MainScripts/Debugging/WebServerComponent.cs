using UnityEngine;

public class WebServerComponent : MonoBehaviour {
  SimpleHTTPServer _server;

  void Awake() {
    var appPath = Application.dataPath;
    var wwwPath = appPath + "/RemotelyFetchedTestAssets";
    _server = new SimpleHTTPServer(wwwPath, 9991);
    Debug.Log("Starting web server... (" + wwwPath + ")");
  }

  void OnDestroy() {
    _server.Stop();
  }
}
