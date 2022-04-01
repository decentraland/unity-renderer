using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Builder
{
    public interface IGenericPopUp
    {
        /// <summary>
        /// This will show a pop up without any title
        /// </summary>
        /// <param name="subtitle"></param>
        /// <param name="okButtonText"></param>
        /// <param name="cancelButtonText"></param>
        /// <param name="OnOk"></param>
        /// <param name="OnCancel"></param>
        void ShowPopUpWithoutTitle(string subtitle, string okButtonText, string cancelButtonText, Action OnOk, Action OnCancel);

        /// <summary>
        /// Disposes the pop up
        /// </summary>
        void Dispose();
    }
}