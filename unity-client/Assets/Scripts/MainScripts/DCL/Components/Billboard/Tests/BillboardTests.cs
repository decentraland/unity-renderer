using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace Tests
{
    public class BillboardTests : IntegrationTestSuite_Legacy
    {
        Billboard billboard;
        string entityId = "e1";


        [UnityTest]
        public IEnumerator AddBillboardComponent()
        {
            yield return CreateComponent(x: false, y: true, z: false);

            Assert.IsFalse(billboard.GetModel().x, "Wrong model data! x should be false.");
            Assert.IsTrue(billboard.GetModel().y, "Wrong model data! y should be true.");
            Assert.IsFalse(billboard.GetModel().z, "Wrong model data! z should be false");

            yield return null;

            yield return null;
        }

        [UnityTest]
        public IEnumerator CheckLookAtPlayer()
        {
            DCLCharacterController.i.PauseGravity();

            yield return CreateComponent(x: true, y: true, z: true);

            yield return billboard.routine;

            Transform entityTransform = scene.entities[entityId].gameObject.transform;
            Vector3 lookAt = GetLookAtVector(billboard.GetModel(), entityTransform);

            yield return null;
            Assert.AreApproximatelyEqual(lookAt.x, entityTransform.forward.x, "billboard entity forward vector should be the same as the calculated one");
            Assert.AreApproximatelyEqual(lookAt.y, entityTransform.forward.y, "billboard entity forward vector should be the same as the calculated one");
            Assert.AreApproximatelyEqual(lookAt.z, entityTransform.forward.z, "billboard entity forward vector should be the same as the calculated one");

            var billboardModel = new Billboard.Model()
            {
                x = true,
                y = false,
                z = true
            };

            yield return TestHelpers.EntityComponentUpdate<Billboard, Billboard.Model>(billboard, billboardModel);

            lookAt = GetLookAtVector(billboardModel, entityTransform);
            Assert.IsTrue(entityTransform.forward == lookAt, "billboard entity forward vector should be the same as the calculated one");

            billboardModel = new Billboard.Model()
            {
                x = false,
                y = false,
                z = false
            };

            yield return TestHelpers.EntityComponentUpdate<Billboard, Billboard.Model>(billboard, billboardModel);

            lookAt = GetLookAtVector(billboardModel, entityTransform);
            Assert.IsTrue(entityTransform.forward == lookAt, "billboard entity forward vector should be the same as the calculated one");

            yield return null;
        }

        [UnityTest]
        public IEnumerator CheckLookAtPlayerWhileTransformMoves()
        {
            DCLCharacterController.i.PauseGravity();

            yield return CreateComponent(x: true, y: true, z: true);

            yield return billboard.routine;

            var entity = scene.entities[entityId];
            Transform entityTransform = entity.gameObject.transform;
            Vector3 lookAt = GetLookAtVector(billboard.GetModel(), entityTransform);

            Assert.AreApproximatelyEqual(lookAt.x, entityTransform.forward.x, "billboard entity forward vector should be the same as the calculated one");
            Assert.AreApproximatelyEqual(lookAt.y, entityTransform.forward.y, "billboard entity forward vector should be the same as the calculated one");
            Assert.AreApproximatelyEqual(lookAt.z, entityTransform.forward.z, "billboard entity forward vector should be the same as the calculated one");

            var billboardModel = new Billboard.Model();

            yield return TestHelpers.EntityComponentUpdate<Billboard, Billboard.Model>(billboard, billboardModel);

            lookAt = GetLookAtVector(billboardModel, entityTransform);
            Assert.IsTrue(entityTransform.forward == lookAt, "billboard entity forward vector should be the same as the calculated one");

            // We simulate a "system" moving the billboard object
            Vector3 position = Vector3.one;
            for (int i = 0; i < 5; i++)
            {
                position.y += i;
                entityTransform.position = position;
                Assert.IsTrue(entityTransform.forward == lookAt, "billboard entity forward vector should be the same as the calculated one");
            }

            yield return null;
        }

        IEnumerator CreateComponent(bool x, bool y, bool z)
        {
            var entity = TestHelpers.CreateSceneEntity(scene, entityId);
            TestHelpers.SetEntityTransform(scene, entity, Vector3.one, Quaternion.identity, Vector3.one);
            yield return null;

            var billboardModel = new Billboard.Model()
            {
                x = x,
                y = y,
                z = z
            };

            billboard =
                TestHelpers.EntityComponentCreate<Billboard, Billboard.Model>(scene, scene.entities[entityId],
                    billboardModel);

            Assert.IsTrue(billboard != null, "Component creation fail!");

            yield return billboard.routine;
        }

        Vector3 GetLookAtVector(Billboard.Model model, Transform entityTransform)
        {
            Vector3 lookAtDir = CommonScriptableObjects.cameraPosition - entityTransform.position;

            // Note (Zak): This check is here to avoid normalizing twice if not needed
            if (!(model.x && model.y && model.z))
            {
                lookAtDir.Normalize();

                // Note (Zak): Model x,y,z are axis that we want to enable/disable
                // while lookAtDir x,y,z are the components of the look-at vector
                if (!model.x || model.z)
                    lookAtDir.y = entityTransform.forward.y;
                if (!model.y)
                    lookAtDir.x = entityTransform.forward.x;
            }

            return lookAtDir.normalized;
        }
    }
}
