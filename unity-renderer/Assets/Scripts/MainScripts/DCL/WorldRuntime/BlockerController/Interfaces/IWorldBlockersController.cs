using System;
using DCL.Rendering;

namespace DCL.Controllers
{
    public interface IWorldBlockersController : IService
    {
        void SetupWorldBlockers();
        void SetEnabled(bool targetValue);
    }
}