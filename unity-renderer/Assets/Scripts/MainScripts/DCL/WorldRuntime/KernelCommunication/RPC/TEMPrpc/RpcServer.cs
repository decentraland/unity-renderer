using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Collections;
using rpc_csharp.protocol;
using rpc_csharp.server;
using rpc_csharp.transport;

namespace rpc_csharp
{
    public class RpcServer<TContext>
    {
        private uint lastPortId = 0;

        private readonly Dictionary<uint, RpcServerPort<TContext>> ports =
            new Dictionary<uint, RpcServerPort<TContext>>();

        private RpcServerHandler<TContext> handler;

        private static StreamMessage reusedStreamMessage = new StreamMessage()
        {
            Closed = false,
            Ack = false,
            Payload = ByteString.Empty
        };

        public RpcServer() { }

        private RpcServerPort<TContext> HandleCreatePort(CreatePort message, uint messageNumber, TContext context,
            ITransport transport)
        {
            ++lastPortId;
            var port = new RpcServerPort<TContext>(lastPortId, message.PortName);
            ports.Add(port.portId, port);

            handler?.Invoke(port, transport, context);

            var response = new CreatePortResponse
            {
                MessageIdentifier = ProtocolHelpers.CalculateMessageIdentifier(
                    RpcMessageTypes.CreatePortResponse,
                    messageNumber
                ),
                PortId = port.portId
            };
            transport.SendMessage(response.ToByteArray());

            return port;
        }

        private async UniTask HandleRequestModule(RequestModule message, uint messageNumber, ITransport transport)
        {
            if (!ports.TryGetValue(message.PortId, out var port))
            {
                throw new InvalidOperationException($"Cannot find port {message.PortId}");
            }

            var loadedModule = await port.LoadModule(message.ModuleName);

            var inProcedures = loadedModule.procedures;

            var pbProcedures = new RepeatedField<ModuleProcedure>
                { };

            int inProceduresCount = inProcedures.Count;
            for (int i = 0; i < inProceduresCount; i++)
            {
                var procedure = inProcedures[i];
                pbProcedures.Add(new ModuleProcedure()
                {
                    ProcedureId = procedure.procedureId,
                    ProcedureName = procedure.procedureName
                });
            }

            var response = new RequestModuleResponse
            {
                Procedures = { pbProcedures },
                MessageIdentifier = ProtocolHelpers.CalculateMessageIdentifier(
                    RpcMessageTypes.RequestModuleResponse,
                    messageNumber
                ),
                PortId = port.portId
            };
            transport.SendMessage(response.ToByteArray());
        }

        private static async UniTask SendStream(AckHelper ackHelper, ITransport transport, uint messageNumber,
            uint portId,
            IEnumerator<ByteString> stream)
        {
            uint sequenceNumber = 0;

            // reset stream message
            reusedStreamMessage.MessageIdentifier = ProtocolHelpers.CalculateMessageIdentifier(
                RpcMessageTypes.StreamMessage,
                messageNumber
            );
            reusedStreamMessage.Closed = false;
            reusedStreamMessage.Ack = false;
            reusedStreamMessage.Payload = ByteString.Empty;
            reusedStreamMessage.PortId = portId;
            reusedStreamMessage.SequenceId = sequenceNumber;

            // First, tell the client that we are opening a stream. Once the client sends
            // an ACK, we will know if they are ready to consume the first element.
            // If the response is instead close=true, then this function returns and
            // no stream.next() is called
            // The following lines are called "stream offer" in the tests.
            {
                var ret = await ackHelper.SendWithAck(reusedStreamMessage);
                if (ret.Closed)
                    return;
                if (!ret.Ack)
                    throw new Exception("Error in logic, ACK must be true");
            }

            // If this point is reached, then the client WANTS to consume an element of the
            // generator
            using (var iterator = stream)
            {
                while (iterator.MoveNext())
                {
                    var elem = iterator.Current;
                    sequenceNumber++;
                    reusedStreamMessage.SequenceId = sequenceNumber;
                    reusedStreamMessage.Payload = elem;

                    var ret = await ackHelper.SendWithAck(reusedStreamMessage);
                    if (ret.Ack)
                    {
                        continue;
                    }
                    else if (ret.Closed)
                    {
                        break;
                    }
                }
            }

            transport.SendMessage(ProtocolHelpers.CloseStreamMessage(messageNumber, sequenceNumber, portId));
        }

        private async UniTask HandleRequest(Request message, uint messageNumber, TContext context,
            ITransport transport, AckHelper ackHelper)
        {
            if (!ports.TryGetValue(message.PortId, out var port))
            {
                throw new InvalidOperationException($"Cannot find port {message.PortId}");
            }

            var (unaryCallSuccess, unaryCallResult) =
                await port.TryCallUnaryProcedure(message.ProcedureId, message.Payload, context);

            if (unaryCallSuccess)
            {
                var response = new Response
                {
                    MessageIdentifier = ProtocolHelpers.CalculateMessageIdentifier(
                        RpcMessageTypes.Response,
                        messageNumber
                    ),
                    Payload = unaryCallResult ?? ByteString.Empty
                };

                transport.SendMessage(response.ToByteArray());
            }
            else if (port.TryCallStreamProcedure(message.ProcedureId, message.Payload, context,
                out IEnumerator<ByteString> streamResult))
            {
                await SendStream(ackHelper, transport, messageNumber, port.portId, streamResult);
            }
            else
            {
                throw new InvalidOperationException($"Unknown type {message.PortId}");
            }
        }

        public void SetHandler(RpcServerHandler<TContext> handler)
        {
            this.handler = handler;
        }

        public void AttachTransport(ITransport transport, TContext context)
        {
            var ackHelper = new AckHelper(transport);

            transport.OnMessageEvent += async (byte[] data) =>
            {
                var parsedMessage = ProtocolHelpers.ParseProtocolMessage(data);

                if (parsedMessage != null)
                {
                    var (messageType, message, messageNumber) = parsedMessage.Value;
                    switch (messageType)
                    {
                        case RpcMessageTypes.CreatePort:
                            HandleCreatePort((CreatePort)message, messageNumber, context, transport);
                            break;
                        case RpcMessageTypes.RequestModule:
                            await HandleRequestModule((RequestModule)message, messageNumber, transport);
                            break;
                        case RpcMessageTypes.Request:
                            await HandleRequest((Request)message, messageNumber, context, transport, ackHelper);
                            break;
                        case RpcMessageTypes.StreamAck:
                        case RpcMessageTypes.StreamMessage:
                            ackHelper.ReceiveAck((StreamMessage)message, messageNumber);
                            break;
                        default:
                            Console.WriteLine("Not implemented message: " + messageType);
                            break;
                    }
                }
            };
        }
    }
}