using AvatarSystem;
using DCL.Helpers;
using DCL.Shaders;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using LOD = AvatarSystem.LOD;

namespace Test.AvatarSystem
{
    public class LODShould
    {
        private static readonly int BASE_COLOR = Shader.PropertyToID("_BaseColor");
        private LOD lod;
        private GameObject impostorContainer;
        private IVisibility visibility;
        private IAvatarMovementController avatarMovementController;

        [SetUp]
        public void SetUp()
        {
            impostorContainer = new GameObject("Impostor container");
            visibility = Substitute.For<IVisibility>();
            avatarMovementController = Substitute.For<IAvatarMovementController>();
            lod = new LOD(impostorContainer, visibility, avatarMovementController);
        }

        [Test, RequiresPlayMode]
        public void EnsureImpostor()
        {
            Assert.Null(lod.impostorRenderer);
            Assert.Null(lod.impostorMeshFilter);

            lod.EnsureImpostor();

            Assert.NotNull(lod.impostorRenderer);
            Assert.NotNull(lod.impostorMeshFilter);
        }

        [Test, RequiresPlayMode]
        [TestCase(1, 0, 0)]
        [TestCase(0, 1, 0)]
        public void TintImpostor(float r, float g, float b)
        {
            lod.EnsureImpostor();
            Color impostorColor = lod.impostorRenderer.material.GetColor(BASE_COLOR);
            Color color = new Color(r, g, b, impostorColor.a); //alpha is ignored
            lod.SetImpostorTint(color);
            impostorColor = lod.impostorRenderer.material.GetColor(BASE_COLOR);
            Assert.AreEqual(color, impostorColor);
        }

        [Test, RequiresPlayMode]
        public void MakeTheBillboardLookAt()
        {
            lod.EnsureImpostor();
            GameObject lookAtTarget = new GameObject("look at target");
            lookAtTarget.transform.position = lod.impostorRenderer.transform.position + (Vector3.forward * 2);

            Vector3 initialEulerAngle = new Vector3(0.7f, 0.7f, 0.7f);
            lod.impostorRenderer.transform.eulerAngles = initialEulerAngle;
            lod.SetBillboardRotation(lookAtTarget.transform);

            Assert.AreEqual(lod.impostorRenderer.transform.eulerAngles.y, 0);

            Utils.SafeDestroy(lookAtTarget);
        }

        [Test,RequiresPlayMode]
        [TestCase(0, true)]
        [TestCase(1, false)]
        [TestCase(2, false)]
        public void UpdateScreenSpaceAmbientOclussion(int lodIndex, bool enabled)
        {
            lod.EnsureImpostor();
            var renderer = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<Renderer>();

            // Default lit material has _SSAO_OFF as an invalid keyword
            renderer.sharedMaterial = Resources.Load<Material>("Avatar Material");

            LOD.UpdateSSAO(renderer, lodIndex);

            for (var i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                //SSAO is disabled explicitly, therefor the negated condition
                bool isSSAOEnabled = !renderer.sharedMaterials[i].IsKeywordEnabled(ShaderUtils.SSAO_OFF_KEYWORD);
                Assert.AreEqual(enabled, isSSAOEnabled);
            }

            Utils.SafeDestroy(renderer.gameObject);
        }

        [Test]
        [TestCase(0, 0f)]
        [TestCase(1, 0f)]
        [TestCase(2, 1.79f)]
        public void UpdateMovementLerping(int lodIndex, float expected)
        {
            lod.UpdateMovementLerping(lodIndex);

            avatarMovementController.Received().SetMovementLerpWait(expected);
        }

        [Test, RequiresPlayMode]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void UpdateAlpha(float avatarAlpha)
        {
            lod.EnsureImpostor();
            var renderer = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<Renderer>();
            lod.combinedAvatar = renderer;
            lod.UpdateAlpha(avatarAlpha);

            foreach (Material material in lod.combinedAvatar.sharedMaterials)
                Assert.AreEqual(avatarAlpha, material.GetFloat(ShaderUtils.DitherFade));

            Material impostorMaterial = lod.impostorRenderer.material;
            float impostorAlpha = impostorMaterial.GetColor(ShaderUtils.BaseColor).a;
            Assert.AreEqual(1f - avatarAlpha, impostorAlpha);

            Utils.SafeDestroy(renderer.gameObject);
        }

        [TearDown]
        public void TearDown() { Utils.SafeDestroy(impostorContainer); }
    }

}
