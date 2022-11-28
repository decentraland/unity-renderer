namespace Altom.AltDriver
{
    public struct AltProperty
    {
        public string name;
        public string value;
        public AltType type;

        public AltProperty(string name, string value, AltType type)
        {
            this.name = name;
            this.value = value;
            this.type = type;
        }
    }
}