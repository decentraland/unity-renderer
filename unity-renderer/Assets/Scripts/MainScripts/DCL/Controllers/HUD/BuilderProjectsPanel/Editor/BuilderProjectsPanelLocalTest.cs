using DCL;
using UnityEngine;
using UnityEngine.EventSystems;

/*
 * NOTE: create BuilderProjectsPanelController with just dragging a prefab in the editor
 * to make it easy to test and modify without the need of loading the whole client
 */
public class BuilderProjectsPanelLocalTest : MonoBehaviour
{
    private BuilderProjectsPanelController controller;

    void Awake()
    {
        WebRequestController.Create();

        controller = new BuilderProjectsPanelController();
    }
    void Start()
    {
        if (EventSystem.current == null)
        {
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
        controller.Initialize();
        controller.SetVisibility(true);
    }
}