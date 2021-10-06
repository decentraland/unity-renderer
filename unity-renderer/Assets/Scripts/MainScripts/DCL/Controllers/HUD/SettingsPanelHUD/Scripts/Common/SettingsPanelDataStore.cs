using DCL.SettingsCommon.SettingsControllers.BaseControllers;

/// <summary>
/// Data Store that contains accessors to all the loaded settings controllers.
/// </summary>
public class SettingsPanelDataStore
{
    private static SettingsPanelDataStore instance = new SettingsPanelDataStore();
    public static SettingsPanelDataStore i { get => instance; }
    public static void Clear() => instance = new SettingsPanelDataStore();

    /// <summary>
    /// Access to all the settings control controllers.
    /// </summary>
    public readonly Controls controls = new Controls();

    public class Controls
    {
        public readonly BaseCollection<SettingsControlController> controlControllers = new BaseCollection<SettingsControlController>();

        public bool TryGetController<T>(out T controlController) where T : SettingsControlController
        {
            bool found = false;
            controlController = null;

            foreach (SettingsControlController controller in controlControllers.Get())
            {
                if (controller is T)
                {
                    controlController = (T)controller;
                    return true;
                }
            }

            return found;
        }
    }
}