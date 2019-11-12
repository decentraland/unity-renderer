using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using DCL.Interface;
using Google.Protobuf;

public class PhysicsCast_Tests : TestsBase
{
    const int ENTITIES_COUNT = 3;
    PB_RayQuery raycastQuery = new PB_RayQuery();

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
        yield return base.InitScene();

        DCLCharacterController.i.SetPosition(startPos);

        raycastQuery.QueryId = "123456";
        raycastQuery.QueryType = queryType;
        raycastQuery.Ray = new PB_Ray();
        raycastQuery.Ray.Direction = new PB_Vector3();
        raycastQuery.Ray.Direction.X = Vector3.back.x;
        raycastQuery.Ray.Direction.Y = Vector3.back.y;
        raycastQuery.Ray.Direction.Z = Vector3.back.z;
        raycastQuery.Ray.Distance = 1000;
        raycastQuery.Ray.Origin = new PB_Vector3();
        raycastQuery.Ray.Origin.X = startPos.x;
        raycastQuery.Ray.Origin.Y = startPos.y;
        raycastQuery.Ray.Origin.Z = startPos.z;
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
        response.queryType = raycastQuery.QueryType;
        response.queryId = raycastQuery.QueryId;
        response.payload = new WebInterface.RaycastHitEntity();
        response.payload.ray = new WebInterface.RayInfo();
        response.payload.ray.origin = new Vector3(raycastQuery.Ray.Origin.X, raycastQuery.Ray.Origin.Y, raycastQuery.Ray.Origin.Z);
        response.payload.ray.distance = raycastQuery.Ray.Distance;
        response.payload.ray.direction = new Vector3(raycastQuery.Ray.Direction.X, raycastQuery.Ray.Direction.Y, raycastQuery.Ray.Direction.Z);
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
                sceneController.ParseQuery("raycast", System.Convert.ToBase64String(raycastQuery.ToByteArray()), scene.sceneData.id);
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

        response.queryType = raycastQuery.QueryType;
        response.queryId = raycastQuery.QueryId;
        response.payload = new WebInterface.RaycastHitEntities();
        response.payload.ray = new WebInterface.RayInfo();
        response.payload.ray.origin = new Vector3(raycastQuery.Ray.Origin.X, raycastQuery.Ray.Origin.Y, raycastQuery.Ray.Origin.Z);
        response.payload.ray.distance = raycastQuery.Ray.Distance;
        response.payload.ray.direction = new Vector3(raycastQuery.Ray.Direction.X, raycastQuery.Ray.Direction.Y, raycastQuery.Ray.Direction.Z);
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
                sceneController.ParseQuery("raycast", System.Convert.ToBase64String(raycastQuery.ToByteArray()), scene.sceneData.id);
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
                    for (int i = 0; i < raycastResponse.payload.payload.entities.Length; i++)
                    {
                        bool found = false;

                        for (int j = 0; j < sceneEvent.payload.payload.entities.Length; j++)
                        {
                            if (raycastResponse.payload.payload.entities[i].entity.entityId == sceneEvent.payload.payload.entities[j].entity.entityId)
                            {
                                found = raycastResponse.payload.payload.entities[i].didHit;
                                break;
                            }
                        }

                        if (!found)
                            return false;
                    }

                    eventTriggered = true;

                    return true;
                }

                return false;
            });

        Assert.IsTrue(eventTriggered);
    }
}

