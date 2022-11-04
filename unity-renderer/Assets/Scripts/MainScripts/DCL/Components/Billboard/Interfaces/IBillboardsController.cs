using UnityEngine;

namespace DCL
{
    public interface IBillboardsController : IService
    {
        void BillboardAdded(GameObject billboardContainer);
        void BillboardRemoved(GameObject billboardContainer);
    }
}