using System;
using DCL.Components;
using DCL.Interface;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AvatarSystem;
using DCL.Configuration;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Emotes;
using DCL.Helpers;
using DCL.Models;
using GPUSkinning;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using Avatar = AvatarSystem.Avatar;
using LOD = AvatarSystem.LOD;

namespace DCL.ECSComponents
{
    public class AvatarShapeComponentHandler : IECSComponentHandler<PBAvatarShape>
    {
     
        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {

        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBAvatarShape model)
        {
   
        }
    }
}