using System.Collections.Generic;
using DCL.CRDT;

namespace KernelCommunication
{
    public static class KernelBinaryMessageSerializer
    {
        public static void Serialize(BinaryWriter binaryWriter, CRDTMessage message)
        {
            CRDTSerializer.Serialize(binaryWriter, message);
        }

        public static void Serialize(BinaryWriter binaryWriter, CRDTProtocol crdt)
        {
            // TODO: implement
            // IReadOnlyList<CRDTMessage> state = crdt.GetState();
            // for (int i = 0; i < state.Count; i++)
            // {
            //     Serialize(binaryWriter, state[i]);
            // }
        }

    }
}
