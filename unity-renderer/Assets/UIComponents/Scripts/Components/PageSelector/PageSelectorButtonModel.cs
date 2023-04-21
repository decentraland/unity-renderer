using System;
using UnityEngine;

namespace UIComponents.Scripts.Components
{
    [Serializable]
    public record PageSelectorButtonModel
    {
        [field: SerializeField]
        public int PageNumber { get; set; }
    }
}
