using UnityEngine;

public class DestroyParticlesOnFinish : MonoBehaviour
{
    private const int INDEX_AMOUNT = 10;

    [SerializeField] private ParticleSystem particles;
    private int index;


    private void Awake()
    {
        if (particles == null)
        {
            Destroy(this);
            return;
        }

        index = Random.Range(0, INDEX_AMOUNT);
    }

    public void FixedUpdate()
    {
        if (Time.frameCount % INDEX_AMOUNT != index)
            return;

        if (particles == null || particles.IsAlive())
          return;

        Destroy(gameObject);
    }
}