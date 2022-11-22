using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Social.Passports
{
    public class PassportPlayerPreviewComponentController
    {
        public IPassportPlayerPreviewComponentView view;

        public PassportPlayerPreviewComponentController(IPassportPlayerPreviewComponentView view)
        {
            this.view = view;
        }
    }
}