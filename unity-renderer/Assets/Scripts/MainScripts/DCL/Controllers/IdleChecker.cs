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
        Vector3 _lastMouseCoordinate = Vector3.zero;
        private int _maxTime = 60;
        private float _lastActivityTime = 0.0f;
        private bool _isIdle = false;
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
            _lastActivityTime = Time.time;
        }
        public void Dispose()
        {
            
        }
        
        public void SetMaxTime(int time)
        {
            _maxTime = time;
        }

        public int GetMaxTime()
        {
            return _maxTime;
        }

        public void Update() {
            Vector3 mouseDelta = Input.mousePosition - _lastMouseCoordinate;
            
            bool mouseMoved = mouseDelta.x < 0;
                
            if (Input.anyKey || mouseMoved)
            {
                _lastActivityTime = Time.time;
            }
            
            _lastMouseCoordinate = Input.mousePosition;
        
            if (_isIdle)
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
            _isIdle = status;

            OnChangeStatus?.Invoke(_isIdle);
            
            DCL.Interface.WebInterface.ReportIdleStateChanged(_isIdle);
        }

        private bool IdleCheck() { return Time.time - _lastActivityTime > _maxTime; }

        public bool isIdle()
        {
            return _isIdle;
        }
    }
}