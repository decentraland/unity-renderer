using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Tests
{
    public static class TestAssets
    {
        private static readonly Dictionary<string, object> _assetSearchCache = new Dictionary<string, object>();

        public static T Get<T>(string assetName) where T : Object
        {
            if (_assetSearchCache.ContainsKey(assetName))
            {
                return (T)_assetSearchCache[assetName];
            }

            foreach (var path in AssetDatabase.GetAllAssetPaths())
            {
                string fullPath = Path.GetFullPath(path);
                if (fullPath.Contains(assetName))
                {
                    var relativePath = Regex.Match(fullPath, "Assets.*").Value;
                    var asset = AssetDatabase.LoadAssetAtPath<T>(relativePath);
                    _assetSearchCache.Add(assetName, asset);
                    return asset;
                }
            }
            
            Debug.LogError($"Couldn't find asset with name <b>{assetName}</b>");
            return default;
        }
    }
}