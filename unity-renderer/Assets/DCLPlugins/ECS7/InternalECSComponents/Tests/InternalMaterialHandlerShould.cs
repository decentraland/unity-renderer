using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.Helpers;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class InternalMaterialHandlerShould
    {
        private InternalMaterialHandler handler;
        private IDCLEntity entity;
        private IParcelScene scene;

        [SetUp]
        public void SetUp()
        {
            handler = new InternalMaterialHandler();
            entity = Substitute.For<IDCLEntity>();
            scene = Substitute.For<IParcelScene>();
        }

        [Test]
        public void NotUpdateRenderersIfNulledInModel()
        {
            GameObject go = new GameObject();
            Renderer renderer = go.AddComponent<MeshRenderer>();

            InternalMaterial model = new InternalMaterial();
            model.renderers = new List<Renderer>() { renderer };
            handler.OnComponentModelUpdated(scene, entity, model);
            Assert.AreEqual(renderer, handler.renderers[0]);

            model = new InternalMaterial();
            model.renderers = null;
            handler.OnComponentModelUpdated(scene, entity, model);
            Assert.AreEqual(renderer, handler.renderers[0]);

            handler.OnComponentRemoved(scene, entity);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void CleanMaterialsOnComponentRemoved()
        {
            Material material = new Material(Utils.EnsureResourcesMaterial("Materials/ShapeMaterial"));

            GameObject go = new GameObject();
            Renderer renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;

            InternalMaterial model = new InternalMaterial();
            model.renderers = new List<Renderer>() { null, renderer, null };
            handler.OnComponentModelUpdated(scene, entity, model);
            handler.OnComponentRemoved(scene, entity);

            Assert.IsNull(renderer.sharedMaterial);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(material);
        }
    }
}