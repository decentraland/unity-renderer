using System;
using System.Collections.Generic;
using System.IO;
using DCL;
using Newtonsoft.Json;
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
            string testSpecName = null;
            bool nextLineIsState = false;
            int lineNumber = 0;

            void ResetCRDT()
            {
                crdt = new CRDTProtocol();
                testSpecName = null;
                nextLineIsState = false;
            }

            foreach (string line in File.ReadLines(filePath))
            {
                lineNumber++;

                if (line == "#")
                    continue;

                if (line.StartsWith("#"))
                {
                    testSpecName ??= line;
                }

                if (line == "# Final CRDT State")
                {
                    nextLineIsState = true;
                    continue;
                }

                if (line.StartsWith("{") && line.EndsWith("}"))
                {
                    if (nextLineIsState)
                    {
                        Dictionary<string, CRDTMessage> inputDictionary = null;
                        try
                        {
                            inputDictionary = JsonConvert.DeserializeObject<Dictionary<string, CRDTMessage>>(line);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Error parsing line for state (ln: {lineNumber}) {line} for file {filePath}, {e}");
                        }

                        if (inputDictionary != null)
                        {
                            Assert.IsTrue(AreStatesEqual(crdt, inputDictionary), $"Final state mismatch in line:{lineNumber} for file {filePath}");
                        }
                        ResetCRDT();
                    }
                    else
                    {
                        try
                        {
                            CRDTMessage msg = JsonConvert.DeserializeObject<CRDTMessage>(line);
                            crdt.ProcessMessage(msg);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Error parsing line for msg (ln: {lineNumber}) {line} for file {filePath}, {e}");
                        }
                    }
                }
            }
        }

        static bool AreStatesEqual(CRDTProtocol crdt, Dictionary<string, CRDTMessage> inputDictionary)
        {
            if (inputDictionary.Count != crdt.state.Count)
            {
                return false;
            }

            foreach (var kvp in inputDictionary)
            {
                if (!crdt.state.TryGetValue(kvp.Key, out CRDTMessage storedMessage))
                    return false;

                if (!(storedMessage.timestamp == kvp.Value.timestamp
                      && CRDTProtocol.IsSameData(storedMessage.data, kvp.Value.data)))
                    return false;
            }

            return true;
        }
    }
}