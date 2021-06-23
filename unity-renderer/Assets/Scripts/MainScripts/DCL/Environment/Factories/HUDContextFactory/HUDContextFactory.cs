namespace DCL
{
    public static class HUDContextFactory
    {
        public static HUDContext CreateDefault() { return new HUDContext(new HUDFactory(), new HUDController()); }
    }
}