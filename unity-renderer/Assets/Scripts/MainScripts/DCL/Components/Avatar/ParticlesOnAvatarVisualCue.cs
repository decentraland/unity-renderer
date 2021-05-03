using DCL;
using UnityEngine;

[RequireComponent(typeof(AvatarRenderer))]
public class ParticlesOnAvatarVisualCue : MonoBehaviour
{
    [SerializeField] private AvatarRenderer.VisualCue avatarVisualCue;
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private bool followAvatar;

    private AvatarRenderer avatarRenderer;
    private void Awake()
    {
        avatarRenderer = GetComponent<AvatarRenderer>();
        if (avatarRenderer == null)
            return;
        avatarRenderer.OnVisualCue += OnVisualCue;
    }

    private void OnVisualCue(AvatarRenderer.VisualCue cue)
    {
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