using System.Collections.Generic;
using DCL.Models;
using UnityEngine;

namespace DCL.Controllers
{
    public interface ISceneBoundsFeedbackStyle
    {
        void CleanFeedback();
        void ApplyFeedback(DCL.Models.MeshesInfo meshesInfo, bool isInsideBoundaries);
        List<Material> GetOriginalMaterials(DCL.Models.MeshesInfo meshesInfo);
    }
}
