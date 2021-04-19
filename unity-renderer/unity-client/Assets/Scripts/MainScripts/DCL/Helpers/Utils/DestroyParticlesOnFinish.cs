using UnityEngine;

public class DestroyParticlesOnFinish : MonoBehaviour
{
    [SerializeField] private ParticleSystem particles;

    public void FixedUpdate()
    {
        if (particles != null && !particles.IsAlive())
        {
            Destroy(gameObject);
        }
    }
}