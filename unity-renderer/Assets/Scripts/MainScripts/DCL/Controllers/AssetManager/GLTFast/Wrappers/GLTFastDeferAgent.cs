using System.Threading.Tasks;
using GLTFast;
using UnityEngine;

namespace DCL.GLTFast.Wrappers
{
    // Note (Kinerius) This is a straight up copy of TimeBudgetPerFrameDeferAgent but it reacts to the renderer being inactive
    [DefaultExecutionOrder(-10)]
    public class GltFastDeferAgent : MonoBehaviour, IDeferAgent
    {
        [SerializeField]
        [Range(.01f, 5f)]
        [Tooltip("Per-frame time budget as fraction of the targeted frame time. Keep it well below 0.5, so there's enough time for other game logic and rendering. A value of 1.0 can lead to dropping a full frame. Even higher values can stall for multiple frames.")]
        private float frameBudget = .5f;

        private float lastTime;
        private float timeBudget = .5f / 30;
        private bool isRendererActive;

        private void Start()
        {
            CommonScriptableObjects.rendererState.OnChange += OnRenderStateChanged;
            OnRenderStateChanged(CommonScriptableObjects.rendererState.Get(), false);
        }

        private void OnRenderStateChanged(bool current, bool previous)
        {
            isRendererActive = current;
            UpdateTimeBudget();
        }

        private void UpdateTimeBudget()
        {
            float targetFrameRate = Application.targetFrameRate;

            if (targetFrameRate < 0)
                targetFrameRate = 30;

            timeBudget = isRendererActive ? frameBudget / targetFrameRate : float.MaxValue;
            ResetLastTime();
        }

        private void Awake()
        {
            UpdateTimeBudget();
        }

        private void Update()
        {
            ResetLastTime();
        }

        private void ResetLastTime()
        {
            lastTime = Time.realtimeSinceStartup;
        }

        public bool ShouldDefer() =>
            !FitsInCurrentFrame(0);

        public bool ShouldDefer(float duration) =>
            !FitsInCurrentFrame(duration);

        private bool FitsInCurrentFrame(float duration) =>
            duration <= timeBudget - (Time.realtimeSinceStartup - lastTime);

        public async Task BreakPoint()
        {
            if (ShouldDefer())
                await Task.Yield();
        }

        public async Task BreakPoint(float duration)
        {
            if (ShouldDefer(duration))
                await Task.Yield();
        }
    }
}
