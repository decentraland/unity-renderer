using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class StickersController : MonoBehaviour
    {
        private const int POOL_PREWARM_COUNT = 10;
        private Dictionary<string, Pool> pools = new Dictionary<string, Pool>();
        private StickersFactory stickersFactory;
        private bool isInHideArea;

        private void Awake()
        {
            stickersFactory = Resources.Load<StickersFactory>("StickersFactory");

            ConfigurePools();
        }

        public void PlaySticker(string id)
        {
            PlaySticker(id, transform.position, Vector3.zero, true);
        }

        /// <summary>
        /// Play a sticker
        /// </summary>
        /// <param name="id"></param>
        /// <param name="position"> if following transform, position must be an offset from the target. Otherwise, it's the particle's world position</param>
        /// <param name="direction"></param>
        /// <param name="followTransform"></param>
        public void PlaySticker(string id, Vector3 position, Vector3 direction, bool followTransform)
        {
            if (stickersFactory == null || !stickersFactory.TryGet(id, out GameObject prefab) || isInHideArea)
                return;

            // TODO(Brian): Mock this system properly through our service locators or plugin system
            if (DCL.Configuration.EnvironmentSettings.RUNNING_TESTS)
                return;

            PoolableObject sticker = pools[id].Get();
            GameObject stickerGo = sticker.gameObject;
            stickerGo.transform.position = position;
            stickerGo.transform.rotation = Quaternion.Euler(prefab.transform.rotation.eulerAngles + direction);

            ReleaseParticlesOnFinish releaser = stickerGo.GetOrCreateComponent<ReleaseParticlesOnFinish>();
            if (releaser != null)
                releaser.Initialize(sticker);

            if (followTransform)
            {
                FollowObject stickerFollow = stickerGo.GetOrCreateComponent<FollowObject>();
                stickerFollow.target = transform;
                stickerFollow.offset = stickerGo.transform.position - transform.position;
            }
        }

        public void ToggleHideArea(bool entered)
        {
            isInHideArea = entered;
        }

        internal void ConfigurePools()
        {
            List<StickersFactory.StickerFactoryEntry> stickers = stickersFactory.GetStickersList();

            foreach (StickersFactory.StickerFactoryEntry stricker in stickers)
            {
                string nameID = $"Sticker {stricker.id}";
                Pool pool = PoolManager.i.GetPool(nameID);
                if (pool == null)
                {
                    pool = PoolManager.i.AddPool(
                        nameID,
                        Instantiate(stricker.stickerPrefab),
                        maxPrewarmCount: POOL_PREWARM_COUNT,
                        isPersistent: true);

                    pool.ForcePrewarm();
                }

                pools.Add(stricker.id, pool);
            }
        }
    }
}