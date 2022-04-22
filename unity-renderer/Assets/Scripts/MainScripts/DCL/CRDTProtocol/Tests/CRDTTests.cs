using System;
using System.IO;
using DCL;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Tests
{
    public class CRDTTests
    {
        [Test]
        public void MessagesProcessedCorrectly()
        {
            var g = AssetDatabase.FindAssets($"t:Script {nameof(CRDTTests)}");
            var filePath = AssetDatabase.GUIDToAssetPath(g[0]);
            var testDir = filePath.Substring(filePath.IndexOf('/') + 1).Replace($"{nameof(CRDTTests)}.cs", string.Empty);
            var searchDir = $"{Application.dataPath}/{testDir}";
            string[] filesPath = Directory.GetFiles(searchDir, "*.test");

            for (int i = 0; i < filesPath.Length; i++)
            {
                ProcessFile(filesPath[i]);
            }
        }

        private void ProcessFile(string filePath)
        {
            CRDTProtocol crdt = new CRDTProtocol();
            foreach (string line in File.ReadLines(filePath))
            {  
                Console.WriteLine(line);  
            }  
        }
    }
}