using System;

namespace DCL.SettingsPanelHUD.Common
{
    public static class CommonSettingsPanelEvents
    {
        public static event Action OnRefreshAllWidgetsSize;
        public static void RaiseRefreshAllWidgetsSize()
        {
            OnRefreshAllWidgetsSize?.Invoke();
        }
    }
}