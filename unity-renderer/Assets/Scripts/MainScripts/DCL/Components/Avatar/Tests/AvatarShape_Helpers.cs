using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

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
            var entity = TestUtils.CreateSceneEntity(scene);
            AvatarShape shape = TestUtils.EntityComponentCreate<AvatarShape, AvatarModel>(scene, entity, model, CLASS_ID_COMPONENT.AVATAR_SHAPE);
            TestUtils.SetEntityTransform(scene, entity, new Vector3(0, 0, 0), Quaternion.identity, Vector3.one);
            return shape;
        }

        public static AvatarModel GetTestAvatarModel(string name, string fileName)
        {
            var avatarjson = File.ReadAllText(TestAssetsUtils.GetPathRaw() + "/Avatar/" + fileName);
            AvatarModel model = JsonUtility.FromJson<AvatarModel>(avatarjson);
            model.name = name;
            return model;
        }
    }
}
