using System;
using UnityEngine;

namespace DCL
{
    public class IdleChecker : IIdleChecker
    {
        Vector3 lastMouseCoordinate = Vector3.zero;
        private int maxTime = 60;
        private float lastActivityTime = 0.0f;
        private bool idle = false;
        private IUpdateEventHandler updateEventHandler;
        public event IIdleChecker.ChangeStatus OnChangeStatus;

        public void Subscribe(IIdleChecker.ChangeStatus callback) { OnChangeStatus += callback; }

        public void Unsubscribe(IIdleChecker.ChangeStatus callback) { OnChangeStatus -= callback; }

        public IdleChecker (IUpdateEventHandler eventHandler = null)
        {
            this.updateEventHandler = eventHandler;
        }

        public void Initialize()
        {
            lastActivityTime = Time.time;

            if (this.updateEventHandler == null)
                this.updateEventHandler = DCL.Environment.i.platform.updateEventHandler;

            updateEventHandler?.AddListener(IUpdateEventHandler.EventType.Update, Update);
        }

        public void SetMaxTime(int time) { maxTime = time; }

        public int GetMaxTime() { return maxTime; }

        public void Update()
        {
            Vector3 mouseDelta = Input.mousePosition - lastMouseCoordinate;

            bool mouseMoved = mouseDelta.sqrMagnitude > 0.0001f;

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

        public bool isIdle() { return idle; }

        public void Dispose()
        {
            updateEventHandler?.RemoveListener(IUpdateEventHandler.EventType.Update, Update);
        }
    }
}