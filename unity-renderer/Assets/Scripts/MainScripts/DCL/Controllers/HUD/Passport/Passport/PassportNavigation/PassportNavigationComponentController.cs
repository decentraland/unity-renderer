using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DCL.Social.Passports
{
    public class PassportNavigationComponentController
    {
        private IPassportNavigationComponentView view;
        
        public PassportNavigationComponentController(IPassportNavigationComponentView view)
        {
            this.view = view;
        }
    }
}