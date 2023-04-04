using System;
using UnityEngine;

namespace DCLServices.Lambdas
{
    [Serializable]
    public abstract class PaginatedResponse
    {
        [SerializeField] internal int pageNum;
        [SerializeField] internal int pageSize;
        [SerializeField] internal int totalAmount;

        public int PageNum => pageNum;
        public int PageSize => pageSize;
        public int TotalAmount => totalAmount;
    }
}
