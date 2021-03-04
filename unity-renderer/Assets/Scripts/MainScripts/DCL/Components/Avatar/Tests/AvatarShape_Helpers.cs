using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityGLTF;

namespace AvatarShape_Tests
{
    public static class AvatarShapeTestHelpers
    {
        public static AvatarShape CreateAvatarShape(ParcelScene scene, string name, string fileName)
        {
            var model = GetTestAvatarModel(name, fileName);

            return CreateAvatarShape(scene, model);
        }

        public static AvatarShape CreateAvatarShape(ParcelScene scene, AvatarModel model)
        {
            GLTFSceneImporter.budgetPerFrameInMilliseconds = float.MaxValue;
            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);
            AvatarShape shape = TestHelpers.EntityComponentCreate<AvatarShape, AvatarModel>(scene, entity, model, CLASS_ID_COMPONENT.AVATAR_SHAPE);
            TestHelpers.SetEntityTransform(scene, entity, new Vector3(0, 0, 0), Quaternion.identity, Vector3.one);
            return shape;
        }

        public static AvatarModel GetTestAvatarModel(string name, string fileName)
        {
            var avatarjson = File.ReadAllText(Utils.GetTestAssetsPathRaw() + "/Avatar/" + fileName);
            AvatarModel model = JsonUtility.FromJson<AvatarModel>(avatarjson);
            model.name = name;
            return model;
        }
    }

    class AvatarRenderer_Mock : AvatarRenderer
    {
        public static Dictionary<string, WearableController_Mock> GetWearableMockControllers(AvatarRenderer renderer)
        {
            var controllers = GetWearableControllers(renderer);
            return controllers.ToDictionary(x => x.Key, x => new WearableController_Mock(x.Value));
        }


        public static Dictionary<string, WearableController> GetWearableControllers(AvatarRenderer renderer)
        {
            var avatarRendererMock = new GameObject("Temp").AddComponent<AvatarRenderer_Mock>();
            avatarRendererMock.CopyFrom(renderer);

            var result =
                avatarRendererMock.wearableControllers.ToDictionary(x => x.Value.id, y => y.Value);

            Destroy(avatarRendererMock.gameObject);

            return result;
        }

        public static WearableController_Mock GetWearableController(AvatarRenderer renderer, string id)
        {
            var wearableControllers = GetWearableControllers(renderer);

            if (!wearableControllers.ContainsKey(id))
                return null;

            return new WearableController_Mock(wearableControllers[id]);
        }

        public static BodyShapeController_Mock GetBodyShapeController(AvatarRenderer renderer)
        {
            var bodyShapeController = GetBodyShape(renderer);
            if (bodyShapeController == null) return null;

            return new BodyShapeController_Mock(bodyShapeController);
        }

        public static BodyShapeController GetBodyShape(AvatarRenderer renderer)
        {
            var avatarRendererMock = new GameObject("Temp").AddComponent<AvatarRenderer_Mock>();
            avatarRendererMock.CopyFrom(renderer);

            var toReturn = avatarRendererMock.bodyShapeController;
            Destroy(avatarRendererMock.gameObject);

            return toReturn;
        }

        protected override void OnDestroy()
        {
        } //Override OnDestroy to prevent mock renderer from resetting the Avatar
    }

    class WearableController_Mock : WearableController
    {
        public WearableController_Mock(WearableItem wearableItem) : base(wearableItem)
        {
        }

        public WearableController_Mock(WearableController original) : base(original)
        {
        }

        public Renderer[] myAssetRenderers => assetRenderers;
        public GameObject myAssetContainer => assetContainer;
        public RendereableAssetLoadHelper myLoader => loader;
    }

    class BodyShapeController_Mock : BodyShapeController
    {
        public BodyShapeController_Mock(WearableItem original) : base(original)
        {
        }

        public BodyShapeController_Mock(BodyShapeController original) : base(original)
        {
        }

        public Renderer[] myAssetRenderers => assetRenderers;
        public GameObject myAssetContainer => this.assetContainer;
    }
}