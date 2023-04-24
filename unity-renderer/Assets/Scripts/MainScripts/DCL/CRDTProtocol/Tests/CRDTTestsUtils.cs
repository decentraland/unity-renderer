using DCL.CRDT;
using DCL.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Tests
{
    internal class CRDTTestsUtils
    {
        public static string[] GetTestFilesPath()
        {
            return Directory.GetFiles(TestAssetsUtils.GetPathRaw() + "/CRDT/", "*.test");
        }

        public static ParsedCRDTTestFile ParseTestFile(string filePath)
        {
            ParsedCRDTTestFile parsedFile = new ParsedCRDTTestFile()
            {
                fileName = filePath
            };

            string testSpecName = null;
            bool nextLineIsState = false;
            int lineNumber = 0;

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

                if (!(line.StartsWith("{") && line.EndsWith("}")))
                    continue;

                if (nextLineIsState)
                {
                    parsedFile.fileInstructions.Add(new ParsedCRDTTestFile.TestFileInstruction()
                    {
                        fileName = filePath,
                        instructionType = ParsedCRDTTestFile.InstructionType.FINAL_STATE,
                        instructionValue = line,
                        lineNumber = lineNumber,
                        testSpect = testSpecName
                    });

                    testSpecName = null;
                    nextLineIsState = false;
                }
                else
                {
                    parsedFile.fileInstructions.Add(new ParsedCRDTTestFile.TestFileInstruction()
                    {
                        fileName = filePath,
                        instructionType = ParsedCRDTTestFile.InstructionType.MESSAGE,
                        instructionValue = line,
                        lineNumber = lineNumber,
                        testSpect = testSpecName
                    });
                }
            }

            return parsedFile;
        }
    }

    internal class CrdtEntity
    {
        public int entityNumber;
        public int entityVersion;
    }

    internal class CrdtJsonState
    {
        public List<CRDTProtocol.CrdtEntityComponentData> components;
        public List<CrdtEntity> deletedEntities;
    }

    public class ParsedCRDTTestFile
    {
        public enum InstructionType
        {
            MESSAGE,
            FINAL_STATE
        }

        public class TestFileInstruction
        {
            public InstructionType instructionType;
            public string instructionValue;
            public int lineNumber;
            public string fileName;
            public string testSpect;
        }

        public class CrdtMessageJson
        {
            public CrdtMessageType type = CrdtMessageType.NONE;
            public long entityId;
            public int componentId;
            public int timestamp;
            public object data;
        }

        public string fileName;
        public List<TestFileInstruction> fileInstructions = new List<TestFileInstruction>();

        public static CrdtMessage InstructionToMessage(TestFileInstruction instruction)
        {
            CrdtMessageJson msg = null;

            try
            {
                msg = JsonConvert.DeserializeObject<CrdtMessageJson>(instruction.instructionValue);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing line for msg (ln: {instruction.lineNumber}) " +
                               $"{instruction.instructionValue} for file {instruction.fileName}, {e}");
            }

            // move from crdt message type from crdt library to crdt protocol
            int crdtLibType = (int)msg.type;

            if (crdtLibType == 1)
            {
                crdtLibType = (int)CrdtMessageType.PUT_COMPONENT;
            }
            else if (crdtLibType == 2)
            {
                crdtLibType = (int)CrdtMessageType.DELETE_ENTITY;
            }

            return new CrdtMessage(
                (CrdtMessageType)crdtLibType,
                msg.entityId,
                msg.componentId,
                msg.timestamp,
                msg.data);
        }

        internal static CRDTProtocol.CrdtState InstructionToFinalState(TestFileInstruction instruction)
        {
            CrdtJsonState finalState = null;

            try
            {
                finalState = JsonConvert.DeserializeObject<CrdtJsonState>(instruction.instructionValue);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing line for state (ln: {instruction.lineNumber}) " +
                               $"{instruction.instructionValue} for file {instruction.fileName}, {e}");
            }

            CRDTProtocol.CrdtState state = new CRDTProtocol.CrdtState();

            foreach (var entityComponentData in finalState.components)
            {
                long entityId = entityComponentData.entityId;
                int componentId = entityComponentData.componentId;

                CRDTProtocol.EntityComponentData realData = new CRDTProtocol.EntityComponentData
                {
                    timestamp = entityComponentData.timestamp,
                    data = entityComponentData.data
                };

                if (!state.singleComponents.TryGetValue(componentId, out var componentCollection))
                {
                    state.singleComponents.Add(componentId, new Dictionary<long, CRDTProtocol.EntityComponentData>());
                }

                state.singleComponents[componentId].Add(entityId, realData);
            }

            foreach (var entity in finalState.deletedEntities)
            {
                state.deletedEntitiesSet.Add(entity.entityNumber, entity.entityVersion);
            }

            return state;
        }
    }
}
