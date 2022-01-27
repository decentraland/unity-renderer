using System;

namespace DCL
{
    public interface IIdleChecker : IService
    {
        void SetMaxTime(int time);
        int GetMaxTime();
        bool isIdle();

        delegate void ChangeStatus(bool isIdle);

        void Subscribe(ChangeStatus callback);
        void Unsubscribe(ChangeStatus callback);
    }
}