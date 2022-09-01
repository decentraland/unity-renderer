using System.Collections.Generic;
using DCL.CRDT;
using NUnit.Framework;

namespace Tests
{
    public class CRDTTests
    {
        [Test]
        public void MessagesProcessedCorrectly()
        {
            string[] filesPath = CRDTTestsUtils.GetTestFilesPath();

            for (int i = 0; i < filesPath.Length; i++)
            {
                ParsedCRDTTestFile parsedFile = CRDTTestsUtils.ParseTestFile(filesPath[i]);
                AssertTestFile(parsedFile);
            }
        }

        private void AssertTestFile(ParsedCRDTTestFile parsedFile)
        {
            CRDTProtocol crdt = new CRDTProtocol();
            for (int i = 0; i < parsedFile.fileInstructions.Count; i++)
            {
                ParsedCRDTTestFile.TestFileInstruction instruction = parsedFile.fileInstructions[i];
                if (instruction.instructionType == ParsedCRDTTestFile.InstructionType.MESSAGE)
                {
                    CRDTMessage msg = ParsedCRDTTestFile.InstructionToMessage(instruction);
                    crdt.ProcessMessage(msg);
                }
                else if (instruction.instructionType == ParsedCRDTTestFile.InstructionType.FINAL_STATE)
                {
                    var finalState = ParsedCRDTTestFile.InstructionToFinalState(instruction);

                    Assert.IsTrue(AreStatesEqual(crdt, finalState), $"Final state mismatch {instruction.testSpect} " +
                                                                    $"in line:{instruction.lineNumber} for file {instruction.fileName}");
                    crdt = new CRDTProtocol();
                }
            }
        }

        static bool AreStatesEqual(CRDTProtocol crdt, IList<CRDTMessage> inputDictionary)
        {
            if (inputDictionary.Count != crdt.state.Count)
            {
                return false;
            }

            foreach (var crdtMessage in inputDictionary)
            {
                if (!crdt.TryGetState(crdtMessage.key1, crdtMessage.key2, out CRDTMessage storedMessage))
                    return false;

                if (!(storedMessage.timestamp == crdtMessage.timestamp
                      && CRDTProtocol.IsSameData(storedMessage.data, crdtMessage.data)))
                    return false;
            }

            return true;
        }
    }
}