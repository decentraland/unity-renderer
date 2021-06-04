using System.Collections.Generic;
using DCL.Controllers;
using DCL.Interface;
using DCL.Models;
using Google.Protobuf;
using UnityEngine;
using DCL.Configuration;

namespace DCL.Bots
{
    public class BotsController : IBotsController
    {
        private ParcelScene globalScene;
        private int botsCount = 0;

        // TODO Implement ClearBots()

        private void EnsureGlobalScene()
        {
            if (globalScene != null)
                return;

            globalScene = GameObject.FindObjectOfType<GlobalScene>(); // TODO: fetch this in a more direct and performant way?
        }

        public void InstantiateBotsAtWorldPos(WorldPosInstantiationConfig config)
        {
            Debug.Log("PRAVS - BotsController - InstantiateBotsAtWorldPos -> " + config);

            EnsureGlobalScene();

            Vector3 randomizedAreaPosition = new Vector3();
            for (int i = 0; i < config.amount; i++)
            {
                randomizedAreaPosition.Set(Random.Range(config.xPos, config.xPos + config.areaWidth), config.yPos, Random.Range(config.zPos, config.zPos + config.areaDepth));
                InstantiateBot(randomizedAreaPosition);
            }
        }

        public void InstantiateBotsAtCoords(CoordsInstantiationConfig config)
        {
            Debug.Log("PRAVS - BotsController - InstantiateBotsAtCoords -> " + config);

            var worldPosConfig = new WorldPosInstantiationConfig()
            {
                amount = config.amount,
                xPos = config.xCoord * ParcelSettings.PARCEL_SIZE,
                yPos = 0f, // TODO: Read player's Y position
                zPos = config.yCoord * ParcelSettings.PARCEL_SIZE,
                areaWidth = config.areaWidth,
                areaDepth = config.areaDepth
            };

            InstantiateBotsAtWorldPos(worldPosConfig);
        }

        void InstantiateBot(Vector3 position)
        {
            AvatarModel avatarModel = new AvatarModel()
            {
                name = "BotAvatar",
                hairColor = Color.white,
                eyeColor = Color.white,
                skinColor = Color.white,
                bodyShape = WearableLiterals.BodyShapes.FEMALE,
                wearables = new List<string>() { }
            };
            // var catalog = AvatarAssetsTestHelpers.CreateTestCatalogLocal();

            string entityId = "BOT-" + botsCount;
            globalScene.CreateEntity(entityId);
            globalScene.EntityComponentCreateOrUpdateWithModel(entityId, CLASS_ID_COMPONENT.AVATAR_SHAPE, avatarModel);
            UpdateEntityTransform(globalScene, entityId, position, Quaternion.identity, Vector3.one);

            botsCount++;
        }

        void UpdateEntityTransform(ParcelScene scene, string entityId, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            PB_Transform pB_Transform = GetPBTransform(position, rotation, scale);
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