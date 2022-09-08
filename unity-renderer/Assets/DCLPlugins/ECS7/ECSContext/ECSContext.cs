namespace DCL.ECS7
{
    public readonly struct ECSContext
    {
        public readonly SystemsContext systemsContext;

        public ECSContext(SystemsContext systemsContext)
        {
            this.systemsContext = systemsContext;
        }
    }
}