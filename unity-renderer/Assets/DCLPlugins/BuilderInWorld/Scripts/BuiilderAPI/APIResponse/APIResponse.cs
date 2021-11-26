using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace DCL.Builder
{
    public class APIResponse
    {
        public bool ok;
        public string error;
        public object data;

        public string GetArrayJsonString()
        {
            return ((JObject) data)["items"].ToString();
        }
    }
}
