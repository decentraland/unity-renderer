using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Components
{
    public interface ILoadable
    {
        System.Action OnSuccess { get; set; }
        System.Action OnFail { get; set; }

        void Load(string url, bool useVisualFeedback);
    }

}
