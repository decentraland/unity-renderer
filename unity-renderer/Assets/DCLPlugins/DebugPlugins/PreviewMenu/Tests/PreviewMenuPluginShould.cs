using NUnit.Framework;

namespace Tests
{
    public class PreviewMenuPluginShould
    {
        [Test]
        public void LoadMenuOnFeatureEnabled()
        {
            var variable = new BaseVariable<bool>();
            variable.Set(true);

            var plugin = new PreviewMenuPlugin(variable);
            Assert.NotNull(plugin.menuController);
            plugin.Dispose();
        }

        [Test]
        public void UnloadMenuOnFeatureDisabled()
        {
            var variable = new BaseVariable<bool>();
            variable.Set(true);

            var plugin = new PreviewMenuPlugin(variable);
            variable.Set(false);

            Assert.IsNull(plugin.menuController);
            plugin.Dispose();
        }

        [Test]
        public void HaveDefaultStateAsDisabled()
        {
            var plugin = new PreviewMenuPlugin();
            Assert.IsNull(plugin.menuController);
            plugin.Dispose();
        }
    }
}