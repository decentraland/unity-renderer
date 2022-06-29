using System.Threading.Tasks;
using UnityEngine;

namespace GLTFast {
    
    // Note (Kinerius) This is a straight up copy of TimeBudgetPerFrameDeferAgent but it reacts to the renderer being inactive
    [DefaultExecutionOrder(-10)]
    public class GLTFastDeferAgent : MonoBehaviour, IDeferAgent {

        [SerializeField]
        [Range(.01f,5f)]
        [Tooltip("Per-frame time budget as fraction of the targeted frame time. Keep it well below 0.5, so there's enough time for other game logic and rendering. A value of 1.0 can lead to dropping a full frame. Even higher values can stall for multiple frames.")]
        float frameBudget = .5f;
        
        float lastTime;
        float timeBudget = .5f/30;
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

        public void SetFrameBudget( float newFrameBudget = 0.5f ) {
            frameBudget = newFrameBudget;
            UpdateTimeBudget();
        }

        void UpdateTimeBudget() {
            float targetFrameRate = Application.targetFrameRate;

            if (targetFrameRate < 0)
                targetFrameRate = 30;
            
            timeBudget = isRendererActive ? frameBudget/targetFrameRate : float.MaxValue;
            ResetLastTime();
        }

        void Awake() {
            UpdateTimeBudget();
        }

        void Update() {
            ResetLastTime();
        }

        void ResetLastTime()
        {
            lastTime = Time.realtimeSinceStartup;
        }
        
        public bool ShouldDefer() {
            return !FitsInCurrentFrame(0);
        }
        
        public bool ShouldDefer( float duration ) {
            return !FitsInCurrentFrame(duration);
        }
        
        bool FitsInCurrentFrame(float duration) {
            return duration <= timeBudget - (Time.realtimeSinceStartup - lastTime);
        }
        
        public async Task BreakPoint() {
            if (ShouldDefer()) {
                await Task.Yield();
            }
        }

        public async Task BreakPoint( float duration ) {
            if (ShouldDefer(duration)) {
                await Task.Yield();
            }
        }
    }
}