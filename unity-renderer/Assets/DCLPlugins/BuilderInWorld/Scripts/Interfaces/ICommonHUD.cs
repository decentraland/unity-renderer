using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Builder
{
    public interface ICommonHUD
    {
        /// <summary>
        /// This will return the instance of the generic pop up
        /// </summary>
        /// <returns></returns>
        IGenericPopUp GetPopUp();

        /// <summary>
        /// Init all the UI elements that are common accros the project
        /// </summary>
        /// <param name="context"></param>
        void Initialize(IContext context);

        /// <summary>
        /// Dispose all the common UI elements 
        /// </summary>
        void Dispose();
    }
}