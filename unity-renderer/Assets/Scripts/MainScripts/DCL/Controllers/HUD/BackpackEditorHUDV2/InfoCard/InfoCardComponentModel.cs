using System;
using System.Collections.Generic;

namespace DCL.Backpack
{
    [Serializable]
    public record InfoCardComponentModel
    {
        public string name;
        public string description;
        public string category;
        public List<string> hideList;
        public List<string> removeList;
    }
}
