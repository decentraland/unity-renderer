using DCL;
using UnityEngine;
using UnityEngine.EventSystems;

/*
 * NOTE: create BuilderProjectsPanelController with just dragging a prefab in the editor
 * to make it easy to test and modify without the need of loading the whole client
 */
public class BuilderProjectsPanelLocalTest : MonoBehaviour
{
    private BuilderMainPanelController controller;

    void Start()
    {
        WebRequestController.Create();
        DataStore.i.builderInWorld.isDevBuild.Set(true);
        controller = new BuilderMainPanelController();
    
        controller.SetVisibility(true);
        
    }
}