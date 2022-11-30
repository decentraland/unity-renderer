using UnityEngine;

namespace Altom.AltTester
{
    [DisallowMultipleComponent]
    public class AltId : MonoBehaviour
    {

        public string altID;
        protected void OnValidate()
        {
            if (altID == null)
                altID = System.Guid.NewGuid().ToString();
        }
    }
}