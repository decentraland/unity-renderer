using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using DCL.Interface;

public class PhysicsCast_Tests : TestsBase
{
    const int ENTITIES_COUNT = 3;
    QueryMessage queryMessage = new QueryMessage();
    Vector3 startPos = new Vector3(5, 5, 15);


    void InstantiateEntityWithShape(Vector3 pos, Vector3 scale, out DecentralandEntity entity, out BoxShape shape)
    {
        shape = TestHelpers.InstantiateEntityWithShape<BoxShape, BoxShape.Model>(
            scene,
            DCL.Models.CLASS_ID.BOX_SHAPE,
            Vector3.zero,
            out entity,
            new BoxShape.Model() { });

        TestHelpers.SetEntityTransform(scene, entity, pos, Quaternion.identity, scale);
    }

    private IEnumerator BaseRaycastTest(string queryType)
    {
        RaycastQuery raycastQuery = new RaycastQuery();

        yield return base.InitScene();

        DCLCharacterController.i.SetPosition(startPos);

        raycastQuery = new RaycastQuery();
        raycastQuery.queryId = "123456";
        raycastQuery.queryType = queryType;
        raycastQuery.sceneId = scene.sceneData.id;
        raycastQuery.ray = new DCL.Models.Ray();
        raycastQuery.ray.direction = Vector3.back;
        raycastQuery.ray.distance = 1000;
        raycastQuery.ray.origin = startPos;

        queryMessage.queryId = "raycast";
        queryMessage.payload = raycastQuery;
    }

    [UnityTest]
    public IEnumerator HitFirst()
    {
        yield return BaseRaycastTest("HitFirst");

        List<DecentralandEntity> entities = new List<DecentralandEntity>();
        Vector3 pos = new Vector3(5, 0, 10);

        for (int i = 0; i < ENTITIES_COUNT; i++)
        {
            DecentralandEntity entity;
            BoxShape shape;

            InstantiateEntityWithShape(pos, new Vector3(5, 10, 1), out entity, out shape);
            yield return shape.routine;

            DCL.CollidersManager.i.ConfigureColliders(entity.meshRootGameObject, true, false, entity);

            entities.Add(entity);

            pos.z -= 2;
        }

        WebInterface.RaycastHitFirstResponse response = new WebInterface.RaycastHitFirstResponse();
        response.queryType = queryMessage.payload.queryType;
        response.queryId = queryMessage.payload.queryId;
        response.payload = new WebInterface.RaycastHitEntity();
        response.payload.ray = new WebInterface.RayInfo();
        response.payload.ray.origin = queryMessage.payload.ray.origin;
        response.payload.ray.distance = queryMessage.payload.ray.distance;
        response.payload.ray.direction = queryMessage.payload.ray.direction;
        response.payload.entity = new WebInterface.HitEntityInfo();
        response.payload.entity.entityId = entities[0].entityId;

        string targetEventType = "SceneEvent";
        var sceneEvent = new WebInterface.SceneEvent<WebInterface.RaycastHitFirstResponse>();
        sceneEvent.sceneId = scene.sceneData.id;
        sceneEvent.payload = response;
        sceneEvent.eventType = "raycastResponse";

        bool eventTriggered = false;

        yield return TestHelpers.WaitForEventFromEngine(targetEventType, sceneEvent,
            () =>
            {
                sceneController.ParseQuery(UnityEngine.JsonUtility.ToJson(queryMessage), scene.sceneData.id);
                //DCL.PhysicsCast.i.Query(raycastQuery);
            },
            (raycastResponse) =>
            {
                Assert.IsTrue(raycastResponse != null);
                Assert.IsTrue(raycastResponse.eventType == sceneEvent.eventType);
                Assert.IsTrue(raycastResponse.sceneId == sceneEvent.sceneId);
                Assert.IsTrue(raycastResponse.payload.queryId == sceneEvent.payload.queryId);
                Assert.IsTrue(raycastResponse.payload.queryType == sceneEvent.payload.queryType);
                Assert.IsTrue(raycastResponse.payload.payload.ray.distance == sceneEvent.payload.payload.ray.distance);
                Assert.IsTrue(raycastResponse.payload.payload.ray.direction == sceneEvent.payload.payload.ray.direction);
                Assert.IsTrue(raycastResponse.payload.payload.ray.origin == sceneEvent.payload.payload.ray.origin);
                Assert.IsTrue(raycastResponse.payload.payload.entity.entityId == sceneEvent.payload.payload.entity.entityId);

                if (raycastResponse != null &&
                    raycastResponse.eventType == sceneEvent.eventType &&
                    raycastResponse.sceneId == sceneEvent.sceneId &&
                    raycastResponse.payload.queryId == sceneEvent.payload.queryId &&
                    raycastResponse.payload.queryType == sceneEvent.payload.queryType &&
                    raycastResponse.payload.payload.ray.distance == sceneEvent.payload.payload.ray.distance &&
                    raycastResponse.payload.payload.ray.direction == sceneEvent.payload.payload.ray.direction &&
                    raycastResponse.payload.payload.ray.origin == sceneEvent.payload.payload.ray.origin &&
                    raycastResponse.payload.payload.entity.entityId == sceneEvent.payload.payload.entity.entityId)
                {
                    eventTriggered = true;
                    return true;
                }

                return false;
            });

        Assert.IsTrue(eventTriggered);
    }

    [UnityTest]
    public IEnumerator HitAll()
    {
        yield return BaseRaycastTest("HitAll");

        List<DecentralandEntity> entities = new List<DecentralandEntity>();
        Vector3 pos = new Vector3(5, 0, 10);

        for (int i = 0; i < ENTITIES_COUNT; i++)
        {
            DecentralandEntity entity;
            BoxShape shape;

            InstantiateEntityWithShape(pos, new Vector3(5, 10, 1), out entity, out shape);
            yield return shape.routine;

            DCL.CollidersManager.i.ConfigureColliders(entity.meshRootGameObject, true, false, entity);

            entities.Add(entity);

            pos.z -= 2;
        }

        WebInterface.RaycastHitAllResponse response = new WebInterface.RaycastHitAllResponse();

        response.queryType = queryMessage.payload.queryType;
        response.queryId = queryMessage.payload.queryId;
        response.payload = new WebInterface.RaycastHitEntities();
        response.payload.ray = new WebInterface.RayInfo();
        response.payload.ray.origin = queryMessage.payload.ray.origin;
        response.payload.ray.distance = queryMessage.payload.ray.distance;
        response.payload.ray.direction = queryMessage.payload.ray.direction;
        response.payload.entities = new WebInterface.RaycastHitEntity[ENTITIES_COUNT];

        for (int i = 0; i < ENTITIES_COUNT; i++)
        {
            response.payload.entities[i] = new WebInterface.RaycastHitEntity();
            response.payload.entities[i].didHit = true;
            response.payload.entities[i].entity = new WebInterface.HitEntityInfo();
            response.payload.entities[i].entity.entityId = entities[i].entityId;
        }

        string targetEventType = "SceneEvent";
        var sceneEvent = new WebInterface.SceneEvent<WebInterface.RaycastHitAllResponse>();
        sceneEvent.sceneId = scene.sceneData.id;
        sceneEvent.payload = response;
        sceneEvent.eventType = "raycastResponse";

        bool eventTriggered = false;

        yield return TestHelpers.WaitForEventFromEngine(targetEventType, sceneEvent,
            () =>
            {
                sceneController.ParseQuery(UnityEngine.JsonUtility.ToJson(queryMessage), scene.sceneData.id);
            },
            (raycastResponse) =>
            {
                Assert.IsTrue(raycastResponse != null);
                Assert.IsTrue(raycastResponse.eventType == sceneEvent.eventType);
                Assert.IsTrue(raycastResponse.sceneId == sceneEvent.sceneId);
                Assert.IsTrue(raycastResponse.payload.queryId == sceneEvent.payload.queryId);
                Assert.IsTrue(raycastResponse.payload.queryType == sceneEvent.payload.queryType);
                Assert.IsTrue(raycastResponse.payload.payload.ray.distance == sceneEvent.payload.payload.ray.distance);
                Assert.IsTrue(raycastResponse.payload.payload.ray.direction == sceneEvent.payload.payload.ray.direction);
                Assert.IsTrue(raycastResponse.payload.payload.ray.origin == sceneEvent.payload.payload.ray.origin);

                if (raycastResponse != null &&
                    raycastResponse.eventType == sceneEvent.eventType &&
                    raycastResponse.sceneId == sceneEvent.sceneId &&
                    raycastResponse.payload.queryId == sceneEvent.payload.queryId &&
                    raycastResponse.payload.queryType == sceneEvent.payload.queryType &&
                    raycastResponse.payload.payload.ray.distance == sceneEvent.payload.payload.ray.distance &&
                    raycastResponse.payload.payload.ray.direction == sceneEvent.payload.payload.ray.direction &&
                    raycastResponse.payload.payload.ray.origin == sceneEvent.payload.payload.ray.origin &&
                    raycastResponse.payload.payload.entities.Length == ENTITIES_COUNT)
                {
                    eventTriggered = true;

                    for (int i = 0; i < raycastResponse.payload.payload.entities.Length; i++)
                    {
                        if (raycastResponse.payload.payload.entities[i].entity.entityId != sceneEvent.payload.payload.entities[i].entity.entityId ||
                            raycastResponse.payload.payload.entities[i].didHit != sceneEvent.payload.payload.entities[i].didHit)
                        {
                            eventTriggered = false;
                            return false;
                        }
                    }
                    return true;
                }

                return false;
            });

        Assert.IsTrue(eventTriggered);
    }
}

