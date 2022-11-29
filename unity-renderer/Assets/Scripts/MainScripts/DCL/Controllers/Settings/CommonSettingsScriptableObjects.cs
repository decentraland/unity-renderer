namespace DCL.SettingsCommon
{
    public static class CommonSettingsScriptableObjects
    {
        private static BooleanVariable shadowsDisabledValue;
        public static BooleanVariable shadowsDisabled => CommonScriptableObjects.GetOrLoad(ref shadowsDisabledValue, "ScriptableObjects/ShadowsDisabled");

        private static BooleanVariable detailObjectCullingDisabledValue;
        public static BooleanVariable detailObjectCullingDisabled => CommonScriptableObjects.GetOrLoad(ref detailObjectCullingDisabledValue, "ScriptableObjects/DetailObjectCullingDisabled");

        // Scriptable object, used by SkyboxTimeControlConfiguration scriptable object in "Flag That Disables Me" to Enable/Disable SkyboxTimeControl UI
        private const string DYNAMIC_SKYBOX_DISABLED_RESOURCE = "ScriptableObjects/DynamicSkyboxDisabled";
        private static BooleanVariable dynamicSkyboxDisabledValue;
        public static BooleanVariable dynamicSkyboxDisabled => CommonScriptableObjects.GetOrLoad(ref dynamicSkyboxDisabledValue, DYNAMIC_SKYBOX_DISABLED_RESOURCE);

        // Scriptable object, used by SkyboxTimeControlConfiguration and DynamicSkyboxControlCOnfiguration scriptable objects in "Flag That Disables Me" to Enable/Disable Skybox Controls UI
        private const string SKYBOX_CONTROLLED_BY_REALM_RESOURCE = "ScriptableObjects/SkyboxControlledByRealm";
        private static BooleanVariable skyboxControlledByRealmValue;
        public static BooleanVariable SkyboxControlledByRealm => CommonScriptableObjects.GetOrLoad(ref skyboxControlledByRealmValue, SKYBOX_CONTROLLED_BY_REALM_RESOURCE);
    }
}
