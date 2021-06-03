using System.Collections.Generic;
using DCL.Controllers;
using DCL.Interface;
using DCL.Models;
using Google.Protobuf;
using UnityEngine;

namespace DCL.Bots
{
    public class BotsController : IBotsController
    {
        public void InstantiateBotsAtWorldPos(WorldPosInstantiationConfig config)
        {
            Debug.Log("PRAVS - BotsController - InstantiateBotsAtWorldPos -> " + config);

            // TODO
        }

        public void InstantiateBotsAtCoords(CoordsInstantiationConfig config)
        {
            Debug.Log("PRAVS - BotsController - InstantiateBotsAtCoords -> " + config);

            /*AvatarModel avatarModel = new AvatarModel()
            {
                name = " test",
                hairColor = Color.white,
                eyeColor = Color.white,
                skinColor = Color.white,
                bodyShape = WearableLiterals.BodyShapes.FEMALE,
                wearables = new List<string>() { }
            };
            // var catalog = AvatarAssetsTestHelpers.CreateTestCatalogLocal();
            
            var globalScene = GameObject.FindObjectOfType<GlobalScene>(); // TODO: fetch this in a more direct and performant way?
            
            var entity = globalScene.CreateEntity("test-avatar-shape");
            globalScene.EntityComponentCreateOrUpdateWithModel(entity.entityId, CLASS_ID_COMPONENT.AVATAR_SHAPE, avatarModel);
            
            UpdateEntityTransform(globalScene, entity.entityId,new Vector3(0, 0, 0), Quaternion.identity, Vector3.one);*/
        }

        void UpdateEntityTransform(ParcelScene scene, string entityId, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            PB_Transform pB_Transform = GetPBTransform(new Vector3(0, 0, 0), Quaternion.identity, Vector3.one);
            scene.EntityComponentCreateOrUpdate(
                entityId,
                CLASS_ID_COMPONENT.TRANSFORM,
                System.Convert.ToBase64String(pB_Transform.ToByteArray())
            );
        }

        public PB_Transform GetPBTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            PB_Transform pbTranf = new PB_Transform();
            pbTranf.Position = new PB_Vector3();
            pbTranf.Position.X = position.x;
            pbTranf.Position.Y = position.y;
            pbTranf.Position.Z = position.z;
            pbTranf.Rotation = new PB_Quaternion();
            pbTranf.Rotation.X = rotation.x;
            pbTranf.Rotation.Y = rotation.y;
            pbTranf.Rotation.Z = rotation.z;
            pbTranf.Rotation.W = rotation.w;
            pbTranf.Scale = new PB_Vector3();
            pbTranf.Scale.X = scale.x;
            pbTranf.Scale.Y = scale.y;
            pbTranf.Scale.Z = scale.z;
            return pbTranf;
        }

        //-----------------------
        /*public AvatarShape CreateAvatarShape(IParcelScene scene, string name, string fileName)
        {
            var model = GetTestAvatarModel(name, fileName);

            return CreateAvatarShape(scene, model);
        }

        public AvatarShape CreateAvatarShape(IParcelScene scene, AvatarModel model)
        {
            GLTFSceneImporter.budgetPerFrameInMilliseconds = float.MaxValue;
            var entity = TestHelpers.CreateSceneEntity(scene);
            AvatarShape shape = TestHelpers.EntityComponentCreate<AvatarShape, AvatarModel>(scene, entity, model, CLASS_ID_COMPONENT.AVATAR_SHAPE);
            TestHelpers.SetEntityTransform(scene, entity, new Vector3(0, 0, 0), Quaternion.identity, Vector3.one);
            return shape;
        }

        public AvatarModel GetTestAvatarModel(string name, string fileName)
        {
            var avatarjson = File.ReadAllText(TestAssetsUtils.GetPathRaw() + "/Avatar/" + fileName);
            AvatarModel model = JsonUtility.FromJson<AvatarModel>(avatarjson);
            model.name = name;
            return model;
        }*/
        //-----------------------
    }
}