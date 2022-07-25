using UnityEngine;

namespace DCL
{
    public class StickersController : MonoBehaviour
    {
        private StickersFactory stickersFactory;
        private bool isInHideArea;

        private void Awake() { stickersFactory = Resources.Load<StickersFactory>("StickersFactory"); }

        public void PlaySticker(string id)
        {
            PlaySticker(id, transform.position, Vector3.zero, true);
        }

        public void PlaySticker(string id, Vector3 position, Vector3 direction, bool followTransform)
        {
            if (stickersFactory == null || !stickersFactory.TryGet(id, out GameObject prefab) || isInHideArea)
                return;

            // TODO(Brian): Mock this system properly through our service locators or plugin system
            if (DCL.Configuration.EnvironmentSettings.RUNNING_TESTS)
                return;

            GameObject emoteGameObject = Instantiate(prefab);
            emoteGameObject.transform.position += position;
            emoteGameObject.transform.rotation = Quaternion.Euler(prefab.transform.rotation.eulerAngles + direction);

            if (followTransform)
            {
                FollowObject emoteFollow = emoteGameObject.AddComponent<FollowObject>();
                emoteFollow.target = transform;
                emoteFollow.offset = prefab.transform.position;
            }
        }

        public void ToggleHideArea(bool entered)
        {
            isInHideArea = entered;
        }
    }
}