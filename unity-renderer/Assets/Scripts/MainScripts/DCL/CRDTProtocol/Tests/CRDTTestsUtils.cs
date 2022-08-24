using System;
using System.Collections.Generic;
using System.IO;
using DCL.CRDT;
using DCL.Helpers;
using Newtonsoft.Json;
using UnityEngine;

namespace Tests
{
    public class CRDTTestsUtils
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

                if (!(line.StartsWith("{") && line.EndsWith("}")) || !(line.StartsWith("[") && line.EndsWith("]")))
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
                        testSpect = string.Empty
                    });
                }
            }
            return parsedFile;
        }
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

        public string fileName;
        public List<TestFileInstruction> fileInstructions = new List<TestFileInstruction>();

        public static CRDTMessage InstructionToMessage(TestFileInstruction instruction)
        {
            CRDTMessage msg = null;
            try
            {
                msg = JsonConvert.DeserializeObject<CRDTMessage>(instruction.instructionValue);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing line for msg (ln: {instruction.lineNumber}) " +
                               $"{instruction.instructionValue} for file {instruction.fileName}, {e}");
            }

            return msg;
        }

        public static IList<CRDTMessage> InstructionToFinalState(TestFileInstruction instruction)
        {
            CRDTMessage[] finalState = null;
            try
            {
                finalState = JsonConvert.DeserializeObject<CRDTMessage[]>(instruction.instructionValue);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing line for state (ln: {instruction.lineNumber}) " +
                               $"{instruction.instructionValue} for file {instruction.fileName}, {e}");
            }
            return finalState;
        }
    }
}