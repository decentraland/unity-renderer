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
        T[] GetArrayFromCall<T>(string result);
        
        /// <summary>
        /// Resolve the data from the call removing de OK response
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        APIResponse GetResponseFromCall(string result);
    }

    public class BuilderAPIResponseResolver: IBuilderAPIResponseResolver
    {
        public T[] GetArrayFromCall<T>(string result)
        {
            APIResponse response = GetResponseFromCall(result);
            string array = response.GetArrayJsonString();
            return JsonConvert.DeserializeObject<T[]>(array);
        }

        public APIResponse GetResponseFromCall(string result)
        {
            APIResponse response = JsonConvert.DeserializeObject<APIResponse>(result);
            return response;
        }
    }
}
