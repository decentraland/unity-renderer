﻿using System;

namespace DCL
{
    public interface IIdleChecker : IDisposable
    {
        void Initialize();
        void SetMaxTime(int time);
        int GetMaxTime();
        bool isIdle();
        void Update();

        delegate void ChangeStatus(bool isIdle);

        void Subscribe(ChangeStatus callback);
        void Unsubscribe(ChangeStatus callback);
    }
}