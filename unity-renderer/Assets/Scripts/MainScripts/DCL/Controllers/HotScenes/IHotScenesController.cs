using Cysharp.Threading.Tasks;
using DCL;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MainScripts.DCL.Controllers.HotScenes
{
    /// <summary>
    /// Despite this service is called `HotScenesController` it is responsible to fetch users outside of comms region,
    /// so it is essentially `ColdScenesController`
    /// </summary>
    public interface IHotScenesController : IService
    {
        UniTask<IReadOnlyList<HotSceneInfo>> GetHotScenesListAsync(CancellationToken cancellationToken);

        [Serializable]
        class HotSceneInfo
        {
            [Serializable]
            public class Realm
            {
                public string serverName;
                public string layer;
                public int usersCount;
                public int maxUsers;
                public Vector2Int[] userParcels;
            }

            public string id;
            public string name;
            public string creator;
            public string description;
            public string thumbnail;
            public Vector2Int baseCoords;
            public Vector2Int[] parcels;
            public int usersTotalCount;
            public Realm[] realms;
        }
    }
}
