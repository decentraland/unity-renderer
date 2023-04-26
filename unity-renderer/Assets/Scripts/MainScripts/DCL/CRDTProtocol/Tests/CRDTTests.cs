using System.Collections.Generic;
using DCL.CRDT;
using NUnit.Framework;
using System.Linq;

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
                    CrdtMessage msg = ParsedCRDTTestFile.InstructionToMessage(instruction);
                    crdt.ProcessMessage(msg);
                }
                else if (instruction.instructionType == ParsedCRDTTestFile.InstructionType.FINAL_STATE)
                {
                    var finalState = ParsedCRDTTestFile.InstructionToFinalState(instruction);
                    bool sameState = AreStatesEqual(crdt.state, finalState, out string reason);

                    Assert.IsTrue(sameState, $"Final state mismatch {instruction.testSpect} " +
                                             $"in line:{instruction.lineNumber} for file {instruction.fileName}. Reason: {reason}");
                    crdt = new CRDTProtocol();
                }
            }
        }

        static bool AreStatesEqual(CRDTProtocol.CrdtState stateA, CRDTProtocol.CrdtState stateB, out string reason)
        {
            // different amount
            int componentDataAmountA = stateA.singleComponents.Aggregate(0, (accum, current) => accum + current.Value.Count);
            int componentDataAmountB = stateB.singleComponents.Aggregate(0, (accum, current) => accum + current.Value.Count);

            if (componentDataAmountA != componentDataAmountB)
            {
                reason = "There is a different amount of entity-component data";
                return false;
            }

            foreach (var componentA in stateA.singleComponents)
            {
                // The component A is not in the state B
                if (!stateB.singleComponents.TryGetValue(componentA.Key, out var componentB))
                {

                    if (stateB.singleComponents.Count == 0)
                    {
                        continue;
                    }

                    reason = $"The component {componentA.Key} from stateA is not in stateB";
                    return false;
                }

                foreach (var entityComponent in componentA.Value)
                {
                    long entityId = entityComponent.Key;
                    var entityComponentDataA = entityComponent.Value;

                    // The entity is in the stateA, but not in stateB
                    if (!componentB.TryGetValue(entityId, out var entityComponentDataB))
                    {
                        reason = $"The entity {entityId} in the component {componentA.Key} from stateA is not in stateB.";
                        return false;
                    }

                    // All good! We know check the data and timestamp.
                    if (entityComponentDataA.timestamp != entityComponentDataB.timestamp)
                    {
                        reason = $"The entity {entityId} in the component {componentA.Key} from stateA has a different TIMESTAMP in the stateB.";
                        return false;
                    }


                    int diff = CRDTProtocol.CompareData(entityComponentDataA.data, entityComponentDataB.data);
                    if (diff != 0)
                    {
                        reason = $"The entity {entityId} in the component {componentA.Key} from stateA has a different DATA in the stateB (cmp(a,b) = {diff}).";
                        return false;
                    }
                }
            }

            if (stateA.deletedEntitiesSet.Count != stateB.deletedEntitiesSet.Count)
            {
                reason = "There is a different amount of deleted entities.";
                return false;
            }

            foreach (var entity in stateA.deletedEntitiesSet)
            {
                if (!stateB.deletedEntitiesSet.TryGetValue(entity.Key, out int entityVersion))
                {
                    reason = $"The entity-number {entity.Key} is deleted in stateA but not in stateB.";
                    return false;
                }

                if (entityVersion != entity.Value)
                {
                    reason = $"The entity-number {entity.Key} has a different deleted version in stateA({entity.Value}) but not in stateB({entityVersion}).";
                    return false;
                }
            }

            reason = "";
            return true;
        }
    }
}
