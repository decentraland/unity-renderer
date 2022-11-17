using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;

public interface IApplicationFocusService : IService
{

    event Action<bool> OnApplicationFocus; 
    bool IsApplicationFocused();


}
