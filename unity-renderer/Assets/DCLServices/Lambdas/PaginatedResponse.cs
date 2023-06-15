using System;
using UnityEngine;

namespace DCLServices.Lambdas
{
    [Serializable]
    public abstract class PaginatedResponse
    {
        public int pageNum;
        public int pageSize;
        public int totalAmount;

        public int PageNum => pageNum;
        public int PageSize => pageSize;
        public int TotalAmount => totalAmount;
    }
}
