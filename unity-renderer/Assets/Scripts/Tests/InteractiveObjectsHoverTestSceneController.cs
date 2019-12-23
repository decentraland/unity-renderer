using DCL;
using DCL.Models;
using DCL.Components;
using DCL.Helpers;
using DCL.Controllers;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine;

public class InteractiveObjectsHoverTestSceneController : MonoBehaviour
{
    ParcelScene scene;

    IEnumerator Start()
    {
        yield return InitScene();

        string entityId = "1";

        var entity = TestHelpers.CreateSceneEntity(scene, entityId);

        string shapeId = TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
            JsonConvert.SerializeObject(new
            {
                src = DCL.Helpers.Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
            }));

        LoadWrapper_GLTF gltfShape = entity.gameObject.GetComponentInChildren<LoadWrapper_GLTF>();
        yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

        TestHelpers.SetEntityTransform(scene, entity, new Vector3(8, -1, 8), Quaternion.identity, new Vector3(0.5f, 0.5f, 0.5f));

        string clickUuid = "pointerevent-1";
        var OnClickComponentModel = new OnClick.Model()
        {
            type = OnClick.NAME,
            uuid = clickUuid,
        };
        TestHelpers.EntityComponentCreate<OnClick, OnClick.Model>(scene, entity, OnClickComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

        scene.SetInitMessagesDone();

        OnPointerEvent.enableInteractionHoverFeedback = true;
    }

    protected virtual IEnumerator InitScene()
    {
        DCL.Configuration.Environment.DEBUG = true;
        SceneController.i.SetDebug();

        TestHelpers.InitializeSceneController(false);

        yield return new WaitForSeconds(0.01f);

        scene = SceneController.i.CreateTestScene();
        yield return null;
    }
}
