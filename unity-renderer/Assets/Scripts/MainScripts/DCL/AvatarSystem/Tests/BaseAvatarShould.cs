using AvatarSystem;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Test.AvatarSystem
{
    public class BaseAvatarShould
    {
        private BaseAvatar baseAvatar;
        private IBaseAvatarReferences references;

        private GameObject smrHolder;
        private GameObject armatureHolder;
        private GameObject particlesHolder;
        private Renderer targetRenderer;

        [SetUp]
        public void SetUp()
        {
            // Prepare BaseAvatar References
            smrHolder = new GameObject("_smrHolder");
            armatureHolder = new GameObject("_armatureHolder");
            particlesHolder = new GameObject("_particlesHolder");
            var smr = smrHolder.AddComponent<SkinnedMeshRenderer>();
            smr.material = new Material(Shader.Find("Unlit/S_FinalGhost"));
            smr.material.SetColor(BaseAvatar.COLOR_ID, Color.white); //Initialize to white to find changes

            // Prepare Target
            targetRenderer = (new GameObject("_targetRenderer")).AddComponent<MeshRenderer>();
            targetRenderer.materials = new[] { new Material(Shader.Find("Unlit/S_FinalGhost")), new Material(Shader.Find("Unlit/S_FinalGhost")) };

            references = Substitute.For<IBaseAvatarReferences>();
            references.Configure().SkinnedMeshRenderer.Returns((_) => smr);
            references.Configure().ArmatureContainer.Returns((_) => armatureHolder.transform);
            references.Configure().ParticlesContainer.Returns((_) => particlesHolder);
            references.Configure().GhostMaxColor.Returns((_) => Color.red);
            references.Configure().GhostMinColor.Returns((_) => Color.blue);
            references.Configure().FadeGhostSpeed.Returns(200); //High values to await quickly
            references.Configure().RevealSpeed.Returns(200);

            baseAvatar = new BaseAvatar(references);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(smrHolder);
            Object.Destroy(armatureHolder);
            Object.Destroy(particlesHolder);
            Object.Destroy(targetRenderer.gameObject);
        }

        [Test]
        public void BeConstructed()
        {
            //Assert
            Assert.AreEqual(references.SkinnedMeshRenderer.material, baseAvatar.ghostMaterial);
            Assert.AreNotEqual(Color.white, baseAvatar.ghostMaterial.GetColor("_Color"));
        }

        [TestCase(0)]
        [TestCase(0.5f)]
        [TestCase(-1)]
        [TestCase(1)]
        public async Task FadeGhost(float initialAlpha)
        {
            var color = baseAvatar.ghostMaterial.GetColor(BaseAvatar.COLOR_ID);
            color.a = initialAlpha;
            baseAvatar.ghostMaterial.SetColor(BaseAvatar.COLOR_ID, color);
            baseAvatar.ghostMaterial.SetVector(BaseAvatar.REVEAL_POSITION_ID, Vector3.one);

            await baseAvatar.FadeGhost();

            Assert.AreEqual(1, baseAvatar.ghostMaterial.GetColor(BaseAvatar.COLOR_ID).a);
            Assert.AreEqual(Vector4.zero, baseAvatar.ghostMaterial.GetVector(BaseAvatar.REVEAL_POSITION_ID));
        }

        [TestCase(0)]
        [TestCase(0.5f)]
        [TestCase(-1)]
        public async Task CancelFadeGhost(float initialAlpha)
        {
            references.Configure().FadeGhostSpeed.Returns(_ => 0.0001f); //Extremely slow to be able to cancel it
            var color = baseAvatar.ghostMaterial.GetColor(BaseAvatar.COLOR_ID);
            color.a = initialAlpha;
            baseAvatar.ghostMaterial.SetColor(BaseAvatar.COLOR_ID, color);
            baseAvatar.ghostMaterial.SetVector(BaseAvatar.REVEAL_POSITION_ID, Vector3.one);
            CancellationTokenSource cts = new CancellationTokenSource();

            var task = baseAvatar.FadeGhost(cts.Token);
            cts.Cancel();
            await task;

            Assert.AreNotEqual(1, baseAvatar.ghostMaterial.GetColor(BaseAvatar.COLOR_ID).a);
        }

        [TestCase(1f, 0)]
        [TestCase(1f, 0.5f)]
        [TestCase(1f, 1f)]
        [TestCase(0.5f, 0)]
        [TestCase(0.5f, 0.5f)]
        [TestCase(0.5f, 1f)]
        public async Task Reveal(float avatarHeight, float initialRevealPos)
        {
            const float REVEAL_OFFSET = 10;
            baseAvatar.ghostMaterial.SetVector(BaseAvatar.REVEAL_POSITION_ID, Vector3.up * initialRevealPos);

            foreach (Material material in targetRenderer.materials)
                material.SetVector(BaseAvatar.REVEAL_POSITION_ID, Vector3.up * initialRevealPos);

            await baseAvatar.Reveal(targetRenderer, avatarHeight, avatarHeight + REVEAL_OFFSET);

            Assert.AreEqual(Vector3.up * (avatarHeight + REVEAL_OFFSET), (Vector3)baseAvatar.ghostMaterial.GetVector(BaseAvatar.REVEAL_POSITION_ID));
            Assert.AreEqual(Vector3.up * -1 * (avatarHeight + REVEAL_OFFSET), (Vector3)targetRenderer.materials[0].GetVector(BaseAvatar.REVEAL_POSITION_ID));
            Assert.AreEqual(Vector3.up * -1 * (avatarHeight + REVEAL_OFFSET), (Vector3)targetRenderer.materials[1].GetVector(BaseAvatar.REVEAL_POSITION_ID));
        }

        [TestCase(1f, 0)]
        [TestCase(1f, 0.5f)]
        [TestCase(1f, 1f)]
        [TestCase(0.5f, 0)]
        [TestCase(0.5f, 0.5f)]
        [TestCase(0.5f, 1f)]
        public async Task CompleteRevealWhenCanceled(float avatarHeight, float initialRevealPos)
        {
            const float REVEAL_OFFSET = 10;
            references.Configure().RevealSpeed.Returns(_ => 0.0001f); //Extremely slow to be able to cancel it
            baseAvatar.ghostMaterial.SetVector(BaseAvatar.REVEAL_POSITION_ID, Vector3.up * initialRevealPos);

            foreach (Material material in targetRenderer.materials)
                material.SetVector(BaseAvatar.REVEAL_POSITION_ID, Vector3.up * initialRevealPos);

            CancellationTokenSource cts = new CancellationTokenSource();

            var task = baseAvatar.Reveal(targetRenderer, avatarHeight, avatarHeight + REVEAL_OFFSET, cts.Token);
            cts.Cancel();
            await task;

            Assert.AreEqual(Vector3.up * (avatarHeight + REVEAL_OFFSET), (Vector3)baseAvatar.ghostMaterial.GetVector(BaseAvatar.REVEAL_POSITION_ID));
            Assert.AreEqual(Vector3.up * -1 * (avatarHeight + REVEAL_OFFSET), (Vector3)targetRenderer.materials[0].GetVector(BaseAvatar.REVEAL_POSITION_ID));
            Assert.AreEqual(Vector3.up * -1 * (avatarHeight + REVEAL_OFFSET), (Vector3)targetRenderer.materials[1].GetVector(BaseAvatar.REVEAL_POSITION_ID));
        }
    }
}
