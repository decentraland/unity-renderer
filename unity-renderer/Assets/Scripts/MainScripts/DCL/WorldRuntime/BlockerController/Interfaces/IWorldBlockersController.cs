using System;
using DCL.Rendering;

namespace DCL.Controllers
{
    public interface IWorldBlockersController : IService
    {
        void SetupWorldBlockers(IParcelScene newScene);
        void RemoveWorldBlockers(IParcelScene newScene);

        void SetEnabled(bool targetValue);
    }
}
