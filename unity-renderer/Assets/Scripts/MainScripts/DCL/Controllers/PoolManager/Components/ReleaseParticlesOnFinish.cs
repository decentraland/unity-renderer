using UnityEngine;

namespace DCL
{
    public class ReleaseParticlesOnFinish : MonoBehaviour
    {
        private const int INDEX_AMOUNT = 10;

        [SerializeField] private ParticleSystem particles;
        private PoolableObject poolable = null;
        private int index;
        private bool initialized;


        public void Initialize(PoolableObject poolableObject)
        {
            if (particles == null)
            {
                Debug.LogError($"The script: {nameof(ReleaseParticlesOnFinish)}, " +
                    $"should never be initialized without particles attached: Aborting initialization");
                Destroy(this);
                return;
            }

            poolable = poolableObject;
            index = Random.Range(0, INDEX_AMOUNT);
            initialized = false;
        }

        public void FixedUpdate()
        {
            if (initialized && Time.frameCount % INDEX_AMOUNT != index)
                return;

            if (particles == null || particles.IsAlive())
                return;

            poolable.Release();
            initialized = false;
        }
    }
}