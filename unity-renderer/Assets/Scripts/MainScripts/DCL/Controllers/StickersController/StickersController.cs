using UnityEngine;

public class StickersController : MonoBehaviour
{
    private StickersFactory stickersFactory;

    private void Awake() { stickersFactory = Resources.Load<StickersFactory>("StickersFactory"); }

    public void PlayEmote(string id)
    {
        PlayEmote(id, transform.position, Vector3.zero, true);
    }

    public void PlayEmote(string id, Vector3 position, Vector3 direction, bool followTransform)
    {
        if (stickersFactory == null || !stickersFactory.TryGet(id, out GameObject prefab))
            return;

        GameObject emoteGameObject = Instantiate(prefab);
        emoteGameObject.transform.position += position;
        emoteGameObject.transform.rotation = Quaternion.Euler(prefab.transform.rotation.eulerAngles + direction);

        if (followTransform) {
            FollowObject emoteFollow = emoteGameObject.AddComponent<FollowObject>();
            emoteFollow.target = transform;
            emoteFollow.offset = prefab.transform.position;
        }
    }
}