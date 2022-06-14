using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using rpc_csharp.protocol;
using rpc_csharp.transport;

namespace rpc_csharp.server
{
    public class AckHelper
    {
        private readonly Dictionary<string, (Action<StreamMessage>, Action<Exception>)> oneTimeCallbacks =
            new Dictionary<string, (Action<StreamMessage>, Action<Exception>)>();

        private readonly ITransport transport;

        public AckHelper(ITransport transport)
        {
            this.transport = transport;

            transport.OnCloseEvent += () =>
            {
                var err = new Exception("Transport closed while waiting the ACK");
                CloseAll(err);
            };

            transport.OnErrorEvent += (err) => { CloseAll(new Exception(err)); };
        }

        private void CloseAll(Exception err)
        {
            using (var iterator = oneTimeCallbacks.Values.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    // reject
                    iterator.Current.Item2(err);
                }
            }

            oneTimeCallbacks.Clear();
        }

        public void ReceiveAck(StreamMessage data, uint messageNumber)
        {
            var key = $"{messageNumber},{data.SequenceId}";
            if (oneTimeCallbacks.TryGetValue(key, out var fut))
            {
                oneTimeCallbacks.Remove(key);
                fut.Item1(data);
            }
        }

        public UniTask<StreamMessage> SendWithAck(StreamMessage data)
        {
            var (_, messageNumber) = ProtocolHelpers.ParseMessageIdentifier(data.MessageIdentifier);
            var key = $"{messageNumber},{data.SequenceId}";

            // C# Promiches
            var ret = new UniTaskCompletionSource<StreamMessage>();
            var accept = new Action<StreamMessage>(message => { ret.TrySetResult(message); });
            var reject = new Action<Exception>(error =>
            {
                Console.WriteLine(error.ToString());
                ret.TrySetException(error);
            });
            oneTimeCallbacks.Add(key, (accept, reject));

            transport.SendMessage(data.ToByteArray());

            return ret.Task;
        }
    }
}