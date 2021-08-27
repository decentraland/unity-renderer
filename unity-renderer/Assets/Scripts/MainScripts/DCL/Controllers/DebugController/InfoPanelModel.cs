using System;

namespace DCL
{
    [Serializable]
    public struct InfoPanelModel
    {
        public bool visible;
        public string[] lines;
        public int textColor; // in encoded hex
        public int backgroundColor; // in encoded hex
    }
}