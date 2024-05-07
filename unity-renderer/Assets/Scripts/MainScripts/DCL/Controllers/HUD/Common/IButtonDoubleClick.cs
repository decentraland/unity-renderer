using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IButtonDoubleClick
{
    bool AlwaysPerformSingleClick { get; set; }

    event Action OnClick;
    event Action OnDoubleClick;
}
