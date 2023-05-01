using System;
using UnityEngine;

namespace DCLServices.Lambdas
{
    [Serializable]
    public abstract class PaginatedResponse
    {
        [SerializeField] protected internal int pageNum;
        [SerializeField] protected internal int pageSize;
        [SerializeField] protected internal int totalAmount;

        public int PageNum => pageNum;
        public int PageSize => pageSize;
        public int TotalAmount => totalAmount;
    }
}
