using UnityEngine;

namespace DCL.Camera
{
    public class InputSpikeFixer
    {
        private const float INPUT_SPIKE_TOLERANCE = 0.5f;

        private CursorLockMode lastLockState;
        private bool isLockStateDirty;

        public InputSpikeFixer()
        {
            lastLockState = Cursor.lockState;
            isLockStateDirty = false;
        }

        public float GetValue(float currentValue)
        {
            CheckLockState();

            float absoluteValue = Mathf.Abs(currentValue);
            
            if (ShouldIgnoreInputValue(absoluteValue))
            {
                if (IsInputValueTolerable(absoluteValue)) 
                    isLockStateDirty = false;
                return 0;
            }
            
            return currentValue;
        }
        private static bool IsInputValueTolerable(float value) { return value < INPUT_SPIKE_TOLERANCE; }
        private bool ShouldIgnoreInputValue(float value) { return value > 0 && isLockStateDirty; }

        private void CheckLockState()
        {
            var currentLockState = Cursor.lockState;

            if (currentLockState != lastLockState)
            {
                isLockStateDirty = true;
            }

            lastLockState = currentLockState;
        }
    }
}