using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine;

public class InteractiveObjectsHoverTestSceneController : MonoBehaviour
{
    public WebInterface.ACTION_BUTTON button = WebInterface.ACTION_BUTTON.POINTER;

    ParcelScene scene;

    IEnumerator Start()
    {
        yield return InitScene();

        long entityId = 1;

        var entity = TestUtils.CreateSceneEntity(scene, entityId);

        string shapeId = TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
            JsonConvert.SerializeObject(new
            {
                src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb"
            }));

        LoadWrapper_GLTF gltfShape = Environment.i.world.state.GetLoaderForEntity(entity) as LoadWrapper_GLTF;
        yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

        TestUtils.SetEntityTransform(scene, entity, new Vector3(8, -1, 8), Quaternion.identity, new Vector3(0.5f, 0.5f, 0.5f));

        var onClickComponentModel = new OnClick.Model()
        {
            type = OnClick.NAME,
            uuid = "pointerevent-1",
            button = this.button.ToString()
        };
        var onClickComponent = TestUtils.EntityComponentCreate<OnClick, OnClick.Model>(scene, entity, onClickComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

        scene.sceneLifecycleHandler.SetInitMessagesDone();

        OnPointerEvent.enableInteractionHoverFeedback = true;
    }

    protected virtual IEnumerator InitScene()
    {
        DCL.Configuration.EnvironmentSettings.DEBUG = true;
        DataStore.i.debugConfig.isDebugMode.Set(true);

        yield return new WaitForSeconds(0.01f);

        scene = TestUtils.CreateTestScene() as ParcelScene;
        yield return null;
    }
}