using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Interface;
using DCL.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BuilderInWorldBridge : MonoBehaviour
{

    public void AddEntityOnKernel(DecentralandEntity entity,ParcelScene scene)
    {
        List<WebInterface.EntityComponentModel> list = new List<WebInterface.EntityComponentModel>();
        foreach (KeyValuePair<CLASS_ID_COMPONENT, BaseComponent> keyValuePair in entity.components)
        {
            if (keyValuePair.Key == CLASS_ID_COMPONENT.TRANSFORM)
            {
                WebInterface.EntityComponentModel entityComponentModel = new WebInterface.EntityComponentModel();
                entityComponentModel.id = (int)CLASS_ID_COMPONENT.TRANSFORM;
                DCLTransform.model.position = SceneController.i.ConvertUnityToScenePosition(entity.gameObject.transform.position, scene);
                DCLTransform.model.rotation = entity.gameObject.transform.rotation;
                DCLTransform.model.scale = entity.gameObject.transform.localScale;

                entityComponentModel.data = "{\"position\":" + JsonUtility.ToJson(DCLTransform.model.position) + "}";
                list.Add(entityComponentModel);

            }
        }

        foreach (KeyValuePair<Type, BaseDisposable> keyValuePair in entity.GetSharedComponents())
        {
            if (keyValuePair.Value is GLTFShape)
            {
                WebInterface.EntityComponentModel entityComponentModel = new WebInterface.EntityComponentModel();
                entityComponentModel.id = (int)CLASS_ID.GLTF_SHAPE;
                GLTFShape gLTFShape = (GLTFShape)keyValuePair.Value;
                entityComponentModel.data = "\"src\": \"" + gLTFShape.model.src + "\"";
                list.Add(entityComponentModel);

            }
        }

        WebInterface.AddEntity(scene.sceneData.id, entity.entityId, list.ToArray());
    }

    public void RemoveEntityOnKernel(string entityId,ParcelScene scene)
    {
        WebInterface.RemoveEntity(scene.sceneData.id, entityId);
    }

    public void StartKernelEditMode(ParcelScene scene)
    {
        WebInterface.ReportControlEvent(new WebInterface.StartStatefulMode(scene.sceneData.id));
    }

    public void ExitKernelEditMode(ParcelScene scene)
    {
        WebInterface.ReportControlEvent(new WebInterface.StopStatefulMode(scene.sceneData.id));
    }

    public void PublishScene(ParcelScene scene)
    {
        WebInterface.ReportStoreSceneState(scene.sceneData.id);
    }

}
