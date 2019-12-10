using DCL;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class BillboardTests : TestsBase
    {
        Billboard billboard;
        string entityId = "e1";


        [UnityTest]
        public IEnumerator AddBillboardComponent()
        {
            yield return InitScene();

            yield return CreateComponent(x: false, y: true, z: false);

            Assert.IsFalse(billboard.model.x, "Wrong model data! x should be false.");
            Assert.IsTrue(billboard.model.y, "Wrong model data! y should be true.");
            Assert.IsFalse(billboard.model.z, "Wrong model data! z should be false");

            yield return null;

            yield return null;
        }

        [UnityTest]
        public IEnumerator CheckLookAtPlayer()
        {
            yield return InitScene();

            DCLCharacterController.i.PauseGravity();

            yield return CreateComponent(x: true, y: true, z: true);

            Transform entityTransform = scene.entities[entityId].gameObject.transform;
            Vector3 lookAt = GetLookAtVector(billboard.model, entityTransform);

            Assert.IsTrue(entityTransform.forward == lookAt, "billboard entity forward vector should be the same as the calculated one");

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

        IEnumerator CreateComponent(bool x, bool y, bool z)
        {
            TestHelpers.CreateSceneEntity(scene, entityId);

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