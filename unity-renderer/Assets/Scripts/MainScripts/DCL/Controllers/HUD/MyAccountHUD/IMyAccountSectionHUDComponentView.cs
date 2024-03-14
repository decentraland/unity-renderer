﻿using System;
using UnityEngine;

namespace DCL.MyAccount
{
    public enum MyAccountSection
    {
        MyProfile,
        EmailNotifications,
        BlockedList,
    }

    public interface IMyAccountSectionHUDComponentView
    {
        void SetAsFullScreenMenuMode(Transform parentTransform);
        void ShowAccountSettingsUpdatedToast();
        void SetSectionsMenuActive(bool isActive);
    }
}
