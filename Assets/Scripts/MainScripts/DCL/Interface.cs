
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using UnityEngine;

namespace DCL.Interface {
  /**
   * This class contains the outgoing interface of Decentraland.
   * You must call those functions to interact with the WebInterface.
   *
   * The messages comming from the WebInterface instead, are reported directly to
   * the handler GameObject by name.
   */
  public static class WebInterface {
#if UNITY_WEBGL && !UNITY_EDITOR
    /**
     * This method is called after the first render. It marks the loading of the
     * rest of the JS client.
     */
    [DllImport("__Internal")] public static extern void StartDecentraland();
    [DllImport("__Internal")] public static extern void MessageFromEngine(string type, string message);
#else
    public static void StartDecentraland() =>
      Debug.Log("StartDecentraland called");

    public static void MessageFromEngine(string type, string message) =>
      Debug.Log("MessageFromEngine called with: " + type + ", " + message);
#endif

    public static void SendMessage(string type, object message) {
      MessageFromEngine(type, UnityEngine.JsonUtility.ToJson(message));
    }

    private static ReportPositionPayload positionPayload = new ReportPositionPayload();

    public static void ReportPosition(Vector3 position, Quaternion rotation) {
      positionPayload.position = position;
      positionPayload.rotation = rotation;

      SendMessage("ReportPosition", positionPayload);
    }

    private class ReportPositionPayload {
      public Vector3 position;
      public Quaternion rotation;
    }
  }
}
