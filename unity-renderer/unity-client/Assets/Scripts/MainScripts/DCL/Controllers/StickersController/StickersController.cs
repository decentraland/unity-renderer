using UnityEngine;

public class StickersController : MonoBehaviour
{
    private StickersFactory stickersFactory;

    private void Awake() { stickersFactory = Resources.Load<StickersFactory>("StickersFactory"); }

    public void PlayEmote(string id)
    {
        if (stickersFactory == null || !stickersFactory.TryGet(id, out GameObject prefab))
            return;

        GameObject emoteGameObject = Instantiate(prefab);
        emoteGameObject.transform.position += transform.position;
        FollowObject emoteFollow = emoteGameObject.AddComponent<FollowObject>();
        emoteFollow.target = transform;
        emoteFollow.offset = prefab.transform.position;
    }
}