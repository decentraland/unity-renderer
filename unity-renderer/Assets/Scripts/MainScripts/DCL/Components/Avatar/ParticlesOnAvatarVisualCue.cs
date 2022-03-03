using DCL;
using UnityEngine;

[RequireComponent(typeof(AvatarRenderer))]
public class ParticlesOnAvatarVisualCue : MonoBehaviour
{
    [SerializeField] private IAvatarRenderer.VisualCue avatarVisualCue;
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private bool followAvatar;
    [SerializeField] private bool ignoreIfFaraway = true;

    private AvatarRenderer avatarRenderer;
    private float lodDistanceSqr;

    private void Awake()
    {
        //If we allow this distance to be changed at runtime, we cannot cache it
        lodDistanceSqr = DataStore.i.avatarsLOD.LODDistance.Get();
        lodDistanceSqr *= lodDistanceSqr;

        avatarRenderer = GetComponent<AvatarRenderer>();
        avatarRenderer.OnVisualCue += OnVisualCue;
    }

    private void OnVisualCue(IAvatarRenderer.VisualCue cue)
    {
        if (ignoreIfFaraway && (avatarRenderer.transform.position - CommonScriptableObjects.playerUnityPosition.Get()).sqrMagnitude > lodDistanceSqr)
            return;

        if (cue != avatarVisualCue || particlePrefab == null)
            return;

        GameObject particles = Instantiate(particlePrefab);
        particles.transform.position = avatarRenderer.transform.position + particlePrefab.transform.position;
        if (followAvatar)
        {
            FollowObject particlesFollow = particles.AddComponent<FollowObject>();
            particlesFollow.target = avatarRenderer.transform;
            particlesFollow.offset = particlePrefab.transform.position;
        }
    }

    private void OnDestroy()
    {
        if (avatarRenderer == null)
            return;
        avatarRenderer.OnVisualCue -= OnVisualCue;
    }
}