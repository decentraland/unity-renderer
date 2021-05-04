using UnityEngine;

namespace DCL
{
    public interface IIdleChecker
    {
        void Initialize();
        void Dispose();
        void SetMaxTime(int time);
        int GetMaxTime();
        bool isIdle();
        void Update();
        
        delegate void ChangeStatus(bool isIdle);
        void Subscribe(ChangeStatus callback);
        void Unsubscribe(ChangeStatus callback);
    }
    
    public class IdleChecker : IIdleChecker
    {
        Vector3 lastMouseCoordinate = Vector3.zero;
        private int maxTime = 60;
        private float lastActivityTime = 0.0f;
        private bool idle = false;
        public event IIdleChecker.ChangeStatus OnChangeStatus;

        public void Subscribe(IIdleChecker.ChangeStatus callback)
        {
            OnChangeStatus += callback;
        }

        public void Unsubscribe(IIdleChecker.ChangeStatus callback)
        {
            OnChangeStatus -= callback;
        }
        
        public void Initialize()
        {
            lastActivityTime = Time.time;
        }
        public void Dispose()
        {
            
        }
        
        public void SetMaxTime(int time)
        {
            maxTime = time;
        }

        public int GetMaxTime()
        {
            return maxTime;
        }

        public void Update() {
            Vector3 mouseDelta = Input.mousePosition - lastMouseCoordinate;
            
            bool mouseMoved = mouseDelta.x < 0;
                
            if (Input.anyKey || mouseMoved)
            {
                lastActivityTime = Time.time;
            }
            
            lastMouseCoordinate = Input.mousePosition;
        
            if (idle)
            {
                if (!IdleCheck())
                    SetIdleState(false);
            }
            else
            {
                if (IdleCheck())
                    SetIdleState(true);
            }
        }
    
        private void SetIdleState(bool status)
        {
            idle = status;

            OnChangeStatus?.Invoke(idle);
            
            DCL.Interface.WebInterface.ReportIdleStateChanged(idle);
        }

        private bool IdleCheck() { return Time.time - lastActivityTime > maxTime; }

        public bool isIdle()
        {
            return idle;
        }
    }
}