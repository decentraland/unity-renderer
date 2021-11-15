using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace DCL.Builder
{
    public interface IBuilderAPIResponseResolver
    {
        /// <summary>
        /// Resolve the data from the call removing de OK response
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        string GetDataFromCall(string result);

        /// <summary>
        ///  Resolve the data from the call removing de OK response and getting only the array
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        string GetDataFromCallArray(string result);
    }

    public class BuilderAPIResponseResolver: IBuilderAPIResponseResolver
    {
        public string GetDataFromCall(string result)
        {
            JObject jObject = (JObject)JsonConvert.DeserializeObject(result);
            if (jObject["ok"].ToObject<bool>())
            {
                return jObject["data"].ToString();
            }
            return "";
        }
    
        public string GetDataFromCallArray(string result)
        {
            JObject jObject = (JObject)JsonConvert.DeserializeObject(result);
            if (jObject["ok"].ToObject<bool>())
            {
                return jObject["data"]["items"].ToString();
            }
            return "";
        }
    }
}
