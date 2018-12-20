using UnityEngine;

public class WebServerComponent : MonoBehaviour {
  static SimpleHTTPServer _server;

  void Awake() {
    if (_server == null) {
      var appPath = Application.dataPath;
      var wwwPath = appPath + "/TestWebserverRoot";
      _server = new SimpleHTTPServer(wwwPath, 9991);
      Debug.Log("Starting web server... (" + wwwPath + ")");
    }
  }

  void OnDestroy() {
    if (_server == null) {
      _server.Stop();
    }
  }
}
