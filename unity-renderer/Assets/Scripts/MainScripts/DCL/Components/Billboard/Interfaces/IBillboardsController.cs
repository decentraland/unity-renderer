using UnityEngine;

namespace DCL
{
    public interface IBillboardsController : IService
    {
        void BillboardAdded(IBillboard billboard);
        void BillboardRemoved(IBillboard billboard);
    }
}