using DCL;
using DCL.Helpers;
using DCL.Components;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using DCL.Controllers;

namespace Tests
{
    public class VideoTests : TestsBase
    {

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            DCLVideoTexture.isTest = true;
        }

        [UnityTest]
        public IEnumerator VideoTextureIsCreatedCorrectly()
        {
            DCLVideoTexture videoTexture = CreateDCLVideoTexture(scene, "it-wont-load-during-test");
            yield return videoTexture.routine;
            Assert.IsTrue(videoTexture.attachedMaterials.Count == 0, "DCLVideoTexture started with attachedMaterials != 0");
        }

        [UnityTest]
        public IEnumerator VideoTextureReplaceOtherTextureCorrectly()
        {
            DCLVideoTexture videoTexture = CreateDCLVideoTexture(scene, "it-wont-load-during-test");
            yield return videoTexture.routine;
            Assert.IsTrue(videoTexture.attachedMaterials.Count == 0, "DCLVideoTexture started with attachedMaterials != 0");

            DCLTexture dclTexture = TestHelpers.CreateDCLTexture(
                  scene,
                  Utils.GetTestsAssetsPath() + "/Images/atlas.png",
                  DCLTexture.BabylonWrapMode.CLAMP,
                  FilterMode.Bilinear);

            yield return dclTexture.routine;

            BasicMaterial mat = TestHelpers.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>
            (scene, CLASS_ID.BASIC_MATERIAL,
                new BasicMaterial.Model
                {
                    texture = dclTexture.id
                });

            yield return mat.routine;

            yield return TestHelpers.SharedComponentUpdate<BasicMaterial, BasicMaterial.Model>(mat, new BasicMaterial.Model() { texture = videoTexture.id });

            Assert.IsTrue(videoTexture.attachedMaterials.Count == 1, $"did DCLVideoTexture attach to material? {videoTexture.attachedMaterials.Count} expected 1");
        }

        [UnityTest]
        public IEnumerator VideoTextureIsAttachedAndDetachedCorrectly()
        {
            DCLVideoTexture videoTexture = CreateDCLVideoTexture(scene, "it-wont-load-during-test");
            yield return videoTexture.routine;
            Assert.IsTrue(videoTexture.attachedMaterials.Count == 0, "DCLVideoTexture started with attachedMaterials != 0");

            BasicMaterial mat2 = TestHelpers.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>
            (scene, CLASS_ID.BASIC_MATERIAL,
                new BasicMaterial.Model
                {
                    texture = videoTexture.id
                });

            yield return mat2.routine;

            Assert.IsTrue(videoTexture.attachedMaterials.Count == 1, $"did DCLVideoTexture attach to material? {videoTexture.attachedMaterials.Count} expected 1");

            // TEST: DCLVideoTexture detach on material disposed
            mat2.Dispose();
            Assert.IsTrue(videoTexture.attachedMaterials.Count == 0, $"did DCLVideoTexture detach from material? {videoTexture.attachedMaterials.Count} expected 0");

            videoTexture.Dispose();

            yield return null;
            Assert.IsTrue(videoTexture.texture == null, "DCLVideoTexture didn't dispose correctly?");
        }

        [UnityTest]
        public IEnumerator VideoTextureVisibleStateIsSetCorrectlyWhenAddedToAMaterialNotAttachedToShape()
        {
            DCLVideoTexture videoTexture = CreateDCLVideoTexture(scene, "it-wont-load-during-test");
            yield return videoTexture.routine;

            DecentralandEntity ent1 = TestHelpers.CreateSceneEntity(scene);
            BasicMaterial ent1Mat = TestHelpers.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>(scene, CLASS_ID.BASIC_MATERIAL, new BasicMaterial.Model() { texture = videoTexture.id });
            TestHelpers.SharedComponentAttach(ent1Mat, ent1);
            yield return ent1Mat.routine;

            Assert.IsTrue(!videoTexture.isVisible, "DCLVideoTexture should not be visible without a shape");
        }

        [UnityTest]
        public IEnumerator VideoTextureVisibleStateIsSetCorrectly()
        {
            DCLVideoTexture videoTexture = CreateDCLVideoTexture(scene, "it-wont-load-during-test");
            yield return videoTexture.routine;

            DecentralandEntity ent1 = TestHelpers.CreateSceneEntity(scene);
            BasicMaterial ent1Mat = TestHelpers.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>(scene, CLASS_ID.BASIC_MATERIAL, new BasicMaterial.Model() { texture = videoTexture.id });
            TestHelpers.SharedComponentAttach(ent1Mat, ent1);
            yield return ent1Mat.routine;

            BoxShape ent1Shape = TestHelpers.SharedComponentCreate<BoxShape, BoxShape.Model>(scene, CLASS_ID.BOX_SHAPE, new BoxShape.Model());
            yield return ent1Shape.routine;

            TestHelpers.SharedComponentAttach(ent1Shape, ent1);
            yield return null; //a frame to wait DCLVideoTexture update    
            Assert.IsTrue(videoTexture.isVisible, "DCLVideoTexture should be visible");

            yield return TestHelpers.SharedComponentUpdate<BoxShape, BoxShape.Model>(ent1Shape, new BoxShape.Model() { visible = false });
            yield return null; //a frame to wait DCLVideoTexture update            

            Assert.IsTrue(!videoTexture.isVisible, "DCLVideoTexture should not be visible ");
        }

        [UnityTest]
        public IEnumerator VideoTextureVisibleStateIsSetCorrectlyWhenAddedToAlreadyAttachedMaterial()
        {
            DCLVideoTexture videoTexture = CreateDCLVideoTexture(scene, "it-wont-load-during-test");
            yield return videoTexture.routine;

            DecentralandEntity ent1 = TestHelpers.CreateSceneEntity(scene);
            BasicMaterial ent1Mat = TestHelpers.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>(scene, CLASS_ID.BASIC_MATERIAL, new BasicMaterial.Model());
            TestHelpers.SharedComponentAttach(ent1Mat, ent1);
            yield return ent1Mat.routine;

            BoxShape ent1Shape = TestHelpers.SharedComponentCreate<BoxShape, BoxShape.Model>(scene, CLASS_ID.BOX_SHAPE, new BoxShape.Model());
            yield return ent1Shape.routine;

            TestHelpers.SharedComponentAttach(ent1Shape, ent1);

            yield return TestHelpers.SharedComponentUpdate<BasicMaterial, BasicMaterial.Model>(ent1Mat, new BasicMaterial.Model() { texture = videoTexture.id });
            yield return null; //a frame to wait DCLVideoTexture update    
            Assert.IsTrue(videoTexture.isVisible, "DCLVideoTexture should be visible");

            yield return TestHelpers.SharedComponentUpdate<BoxShape, BoxShape.Model>(ent1Shape, new BoxShape.Model() { visible = false });
            yield return null; //a frame to wait DCLVideoTexture update            

            Assert.IsTrue(!videoTexture.isVisible, "DCLVideoTexture should not be visible ");
        }

        [UnityTest]
        public IEnumerator VideoTextureVisibleStateIsSetCorrectlyWhenEntityIsRemoved()
        {
            DCLVideoTexture videoTexture = CreateDCLVideoTexture(scene, "it-wont-load-during-test");
            yield return videoTexture.routine;

            DecentralandEntity ent1 = TestHelpers.CreateSceneEntity(scene);
            BasicMaterial ent1Mat = TestHelpers.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>(scene, CLASS_ID.BASIC_MATERIAL, new BasicMaterial.Model() { texture = videoTexture.id });
            TestHelpers.SharedComponentAttach(ent1Mat, ent1);
            yield return ent1Mat.routine;

            BoxShape ent1Shape = TestHelpers.SharedComponentCreate<BoxShape, BoxShape.Model>(scene, CLASS_ID.BOX_SHAPE, new BoxShape.Model());
            yield return ent1Shape.routine;

            TestHelpers.SharedComponentAttach(ent1Shape, ent1);
            yield return null; //a frame to wait DCLVideoTexture update    
            Assert.IsTrue(videoTexture.isVisible, "DCLVideoTexture should be visible");

            scene.RemoveEntity(ent1.entityId, true);
            yield return null; //a frame to wait DCLVideoTexture update            

            Assert.IsTrue(!videoTexture.isVisible, "DCLVideoTexture should not be visible ");
        }

        static DCLVideoClip CreateDCLVideoClip(ParcelScene scn, string url)
        {
            return TestHelpers.SharedComponentCreate<DCLVideoClip, DCLVideoClip.Model>
            (
                scn,
                DCL.Models.CLASS_ID.VIDEO_CLIP,
                new DCLVideoClip.Model
                {
                    url = url
                }
            );
        }

        static DCLVideoTexture CreateDCLVideoTexture(ParcelScene scn, DCLVideoClip clip)
        {
            return TestHelpers.SharedComponentCreate<DCLVideoTexture, DCLVideoTexture.Model>
            (
                scn,
                DCL.Models.CLASS_ID.VIDEO_TEXTURE,
                new DCLVideoTexture.Model
                {
                    videoClipId = clip.id
                }
            );
        }

        static DCLVideoTexture CreateDCLVideoTexture(ParcelScene scn, string url)
        {
            return CreateDCLVideoTexture(scn, CreateDCLVideoClip(scn, "http://" + url));
        }
    }
}