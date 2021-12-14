using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Builder
{
    public interface INewProjectDetailController
    {
        /// <summary>
        /// Set the view active
        /// </summary>
        /// <param name="isActive"></param>
        void SetActive(bool isActive);

        void Initialize(INewProjectDetailView view);
    }

    public class NewProjectDetailController : INewProjectDetailController
    {
        private INewProjectDetailView view;

        public void SetActive(bool isActive) { }
        public void Initialize(INewProjectDetailView missing_name) { throw new System.NotImplementedException(); }
    }
}