using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassportNavigationComponentController
{
    private IPassportNavigationComponentView view;
    
    public PassportNavigationComponentController(IPassportNavigationComponentView view)
    {
        this.view = view;
    }
}
