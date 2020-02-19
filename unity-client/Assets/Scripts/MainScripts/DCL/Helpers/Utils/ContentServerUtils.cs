using UnityEngine;

namespace DCL
{
    public static class ContentServerUtils
    {
        [System.Serializable]
        public class ScenesAPIData
        {
            [System.Serializable]
            public class Data
            {
                public string parcel_id;
                public string root_cid;
                public string scene_cid;
            }

            public Data[] data;
        }

        [System.Serializable]
        public class MappingsAPIData
        {
            [System.Serializable]
            public class Data
            {
                [System.Serializable]
                public class Content
                {
                    public MappingPair[] contents;
                }

                public Content content;
            }

            public Data[] data;
        }

        [System.Serializable]
        public class MappingPair
        {
            public string file;
            public string hash;
        }

        public enum ApiEnvironment
        {
            NONE,
            TODAY,
            ZONE,
            ORG,
        }


        public static string GetEnvString(ApiEnvironment env)
        {
            switch (env)
            {
                case ApiEnvironment.NONE:
                    break;
                case ApiEnvironment.TODAY:
                    return "today";
                case ApiEnvironment.ZONE:
                    return "zone";
                case ApiEnvironment.ORG:
                    return "org";
            }

            return "org";
        }

        public static string customBaseUrl = "";

        public static string GetBaseUrl(ApiEnvironment env)
        {
            if (env != ApiEnvironment.NONE)
            {
                string envString = GetEnvString(env);
                return $"https://content.decentraland.{envString}";
            }
            else
            {
                return customBaseUrl;
            }
        }

        public static string GetScenesAPIUrl(ApiEnvironment env, int x1, int y1, int width, int height)
        {
            width = Mathf.Max(0, width - 1);
            height = Mathf.Max(0, height - 1);

            string baseUrl = GetBaseUrl(env);
            string result = $"{baseUrl}/scenes?x1={x1}&x2={x1 + width}&y1={y1}&y2={y1 + height}";
            Debug.Log($"Using scenes API url {result}");
            return result;
        }

        public static string GetMappingsAPIUrl(ApiEnvironment env, string cid)
        {
            string baseUrl = GetBaseUrl(env);
            string result = $"{baseUrl}/parcel_info?cids={cid}";
            Debug.Log($"Using mappings url {result}");
            return result;
        }

        public static string GetContentAPIUrlBase(ApiEnvironment env)
        {
            string baseUrl = GetBaseUrl(env);
            return $"{baseUrl}/contents/";
        }
    }
}
