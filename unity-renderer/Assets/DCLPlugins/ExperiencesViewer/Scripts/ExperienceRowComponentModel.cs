using System;
using UnityEngine;

namespace DCL.ExperiencesViewer
{
    [Serializable]
    public class ExperienceRowComponentModel : BaseComponentModel
    {
        public string id;
        public string iconUri;
        public string name;
        public bool isUIVisible;
        public bool isPlaying;
        public Color backgroundColor;
        public Color onHoverColor;
        public bool allowStartStop = true;
    }
}