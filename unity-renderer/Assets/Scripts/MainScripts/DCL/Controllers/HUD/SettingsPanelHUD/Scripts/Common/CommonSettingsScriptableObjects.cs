namespace DCL.SettingsPanelHUD.Common
{
    public static class CommonSettingsScriptableObjects
    {
        private static BooleanVariable shadowsDisabledValue;
        public static BooleanVariable shadowsDisabled => CommonScriptableObjects.GetOrLoad(ref shadowsDisabledValue, "ScriptableObjects/ShadowsDisabled");

        private static BooleanVariable detailObjectCullingDisabledValue;
        public static BooleanVariable detailObjectCullingDisabled => CommonScriptableObjects.GetOrLoad(ref detailObjectCullingDisabledValue, "ScriptableObjects/DetailObjectCullingDisabled");
    }
}