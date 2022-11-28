namespace Altom.AltTesterEditor
{
    [System.Serializable]
    public class AltMyScenes
    {
        public bool _toBeBuilt;
        public string _path;
        public int _buildIndex;

        public AltMyScenes(bool beBuilt, string path, int buildIndex)
        {
            _toBeBuilt = beBuilt;
            _path = path;
            _buildIndex = buildIndex;
        }

        public bool ToBeBuilt
        {
            get { return _toBeBuilt; }
            set { _toBeBuilt = value; }
        }

        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        public int BuildScene
        {
            get { return _buildIndex; }
            set { _buildIndex = value; }
        }
    }
}