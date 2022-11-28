namespace Altom.AltDriver
{
    public class AltObjectLight
    {
        public string name;
        public int id;
        public bool enabled;
        public int idCamera;
        public int transformParentId;
        public int transformId;

        public AltObjectLight(string name, int id = 0, bool enabled = true, int idCamera = 0, int transformParentId = 0, int transformId = 0)
        {
            this.name = name;
            this.id = id;
            this.enabled = enabled;
            this.idCamera = idCamera;
            this.transformParentId = transformParentId;
            this.transformId = transformId;
        }
    }
}