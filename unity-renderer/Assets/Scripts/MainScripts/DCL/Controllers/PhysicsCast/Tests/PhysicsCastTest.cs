using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using Google.Protobuf;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

public class PhysicsCast_Tests : TestsBase
{
    const int ENTITIES_COUNT = 3;
    PB_RayQuery raycastQuery;
    PB_Query query;
    PB_SendSceneMessage sendSceneMessage;
    Vector3 startPos = new Vector3(5, 2, 15);
    bool alreadyInitialized = false;

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        Environment.i.world.sceneBoundsChecker.Stop();
    }

    private void ConfigureRaycastQuery(string queryType)
    {
        DCLCharacterController.i.SetPosition(startPos);

        raycastQuery = new PB_RayQuery();
        raycastQuery.QueryId = "123456" + queryType;
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

        query = new PB_Query();
        query.QueryId = "raycast";
        query.Payload = System.Convert.ToBase64String(raycastQuery.ToByteArray());

        sendSceneMessage = new PB_SendSceneMessage();
        sendSceneMessage.Query = query;
        sendSceneMessage.SceneId = this.scene.sceneData.id;
    }

    [UnityTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator HitFirst()
    {
        ConfigureRaycastQuery("HitFirst");

        List<DecentralandEntity> entities = new List<DecentralandEntity>();
        Vector3 pos = new Vector3(5, 0, 10);

        for (int i = 0; i < ENTITIES_COUNT; i++)
        {
            BoxShape shape = TestHelpers.CreateEntityWithBoxShape(scene, pos);
            yield return shape.routine;

            DecentralandEntity entity = shape.attachedEntities.First();

            TestHelpers.SetEntityTransform(scene, entity, pos, Quaternion.identity, new Vector3(5, 10, 1));
            yield return null;

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

        var sceneEvent = new WebInterface.SceneEvent<WebInterface.RaycastHitFirstResponse>();

        bool eventTriggered = false;
        int responseCount = 0;

        yield return SendRaycastQueryMessage<WebInterface.RaycastHitFirstResponse, WebInterface.RaycastHitEntity>(
            sceneEvent, response,
            (raycastResponse) =>
            {
                responseCount++;
                Assert.IsTrue(responseCount == 1, "This raycast query should be lossy and therefore excecuted once.");
                Assert.IsTrue(raycastResponse != null);
                Assert.IsTrue(raycastResponse.payload.payload.entity.entityId == sceneEvent.payload.payload.entity.entityId);

                if (raycastResponse != null &&
                    AreSceneEventsEqual<WebInterface.RaycastHitFirstResponse, WebInterface.RaycastHitEntity>(raycastResponse, sceneEvent) &&
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
    [Explicit]
    [Category("Explicit")]
    public IEnumerator HitAll()
    {
        ConfigureRaycastQuery("HitAll");

        List<DecentralandEntity> entities = new List<DecentralandEntity>();
        Vector3 pos = new Vector3(5, 0, 10);

        for (int i = 0; i < ENTITIES_COUNT; i++)
        {
            BoxShape shape = TestHelpers.CreateEntityWithBoxShape(scene, pos);
            yield return shape.routine;

            DecentralandEntity entity = shape.attachedEntities.First();

            TestHelpers.SetEntityTransform(scene, entity, pos, Quaternion.identity, new Vector3(5, 10, 1));
            yield return null;

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

        var sceneEvent = new WebInterface.SceneEvent<WebInterface.RaycastHitAllResponse>();

        bool eventTriggered = false;
        int responseCount = 0;

        yield return SendRaycastQueryMessage<WebInterface.RaycastHitAllResponse, WebInterface.RaycastHitEntities>(
            sceneEvent, response,
            (raycastResponse) =>
            {
                responseCount++;

                Assert.IsTrue(responseCount == 1, "This raycast query should be lossy and therefore excecuted once.");

                Assert.IsTrue(raycastResponse != null);

                Assert.IsTrue(raycastResponse.payload.payload.entities.Length == ENTITIES_COUNT);

                if (raycastResponse != null &&
                    AreSceneEventsEqual<WebInterface.RaycastHitAllResponse, WebInterface.RaycastHitEntities>(raycastResponse, sceneEvent) &&
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

        yield return null;

        Assert.IsTrue(eventTriggered);
    }

    private IEnumerator SendRaycastQueryMessage<T, T2>
    (
        WebInterface.SceneEvent<T> sceneEvent,
        T response,
        System.Func<WebInterface.SceneEvent<T>, bool> OnMessageReceived
    )
        where T2 : WebInterface.RaycastHitInfo
        where T : WebInterface.RaycastResponse<T2>
    {
        string targetEventType = "SceneEvent";

        sceneEvent.sceneId = scene.sceneData.id;
        sceneEvent.payload = response;
        sceneEvent.eventType = "raycastResponse";

        yield return TestHelpers.ExpectMessageToKernel(
            targetEventType,
            sceneEvent,
            () =>
            {
                // Note (Zak): we send several times the same message to ensure it's
                // only processed once (lossy messages)
                sceneController.SendSceneMessage(System.Convert.ToBase64String(sendSceneMessage.ToByteArray()));
                sceneController.SendSceneMessage(System.Convert.ToBase64String(sendSceneMessage.ToByteArray()));
                sceneController.SendSceneMessage(System.Convert.ToBase64String(sendSceneMessage.ToByteArray()));
            },
            OnMessageReceived);
    }

    private bool AreSceneEventsEqual<T, T2>(WebInterface.SceneEvent<T> s1, WebInterface.SceneEvent<T> s2) where T2 : WebInterface.RaycastHitInfo where T : WebInterface.RaycastResponse<T2>
    {
        Assert.IsTrue(s1.eventType == s2.eventType);
        Assert.IsTrue(s1.sceneId == s2.sceneId);

        if (s1.eventType != s2.eventType ||
            s1.sceneId != s2.sceneId)
            return false;

        return AreRaycastResponsesEqual(s1.payload, s2.payload);
    }

    private bool AreRaycastResponsesEqual<T>(WebInterface.RaycastResponse<T> r1, WebInterface.RaycastResponse<T> r2) where T : WebInterface.RaycastHitInfo
    {
        Assert.IsTrue(r1.queryId == r2.queryId);
        Assert.IsTrue(r1.queryType == r2.queryType);
        Assert.IsTrue(r1.payload.ray.distance == r2.payload.ray.distance);
        Assert.IsTrue(r1.payload.ray.direction == r2.payload.ray.direction);
        Assert.IsTrue(r1.payload.ray.origin == r2.payload.ray.origin);

        return r1.queryId == r2.queryId &&
               r1.queryType == r2.queryType &&
               r1.payload.ray.distance == r2.payload.ray.distance &&
               r1.payload.ray.direction == r2.payload.ray.direction &&
               r1.payload.ray.origin == r2.payload.ray.origin;
    }
}