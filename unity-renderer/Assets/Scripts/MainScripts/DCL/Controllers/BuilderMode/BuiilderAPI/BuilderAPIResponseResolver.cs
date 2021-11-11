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
        string GetDataFromCall(string result, bool isArray = false);
        
        /// <summary>
        /// Resolve the data from the call removing de OK response
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        APIResponse GetResponseFromCall(string result, bool isArray = false);
    }

    public class BuilderAPIResponseResolver: IBuilderAPIResponseResolver
    {
   
        public string GetDataFromCall(string result,bool isArray = false)
        {
            JObject jObject = (JObject)JsonConvert.DeserializeObject(result);
            if (jObject["ok"].ToObject<bool>())
            {
                if (isArray)
                    return jObject["data"]["items"].ToString();
                
                return jObject["data"].ToString();
            }
            return "";
        }

        public APIResponse GetResponseFromCall(string result, bool isArray = false)
        {
            APIResponse response = JsonConvert.DeserializeObject<APIResponse>(result);
            return response;
        }
    }
}
