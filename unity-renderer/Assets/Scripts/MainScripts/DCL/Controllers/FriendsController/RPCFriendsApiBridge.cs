using Cysharp.Threading.Tasks;
using DCL;
using DCL.Social.Friends;
using Decentraland.Renderer.KernelServices;
using Decentraland.Renderer.RendererServices;
using Decentraland.Renderer.Common;
using JetBrains.Annotations;
using RPC;
using rpc_csharp;
using System;
using System.Linq;
using System.Threading;
using FriendshipStatus = DCL.Social.Friends.FriendshipStatus;
using KernelGetFriendRequestsPayload = Decentraland.Renderer.KernelServices.GetFriendRequestsPayload;
using KernelRejectFriendRequestReply = Decentraland.Renderer.KernelServices.RejectFriendRequestReply;
using KernelRejectFriendRequestPayload = Decentraland.Renderer.KernelServices.RejectFriendRequestPayload;
using KernelCancelFriendRequestReply = Decentraland.Renderer.KernelServices.CancelFriendRequestReply;
using KernelCancelFriendRequestPayload = Decentraland.Renderer.KernelServices.CancelFriendRequestPayload;
using RendererRejectFriendRequestReply = Decentraland.Renderer.RendererServices.RejectFriendRequestReply;
using RendererRejectFriendRequestPayload = Decentraland.Renderer.RendererServices.RejectFriendRequestPayload;
using RendererCancelFriendRequestReply = Decentraland.Renderer.RendererServices.CancelFriendRequestReply;
using RendererCancelFriendRequestPayload = Decentraland.Renderer.RendererServices.CancelFriendRequestPayload;
using RPCFriendshipStatus = Decentraland.Renderer.KernelServices.FriendshipStatus;

namespace DCl.Social.Friends
{
    public class RPCFriendsApiBridge : IFriendsApiBridge, IFriendRequestRendererService<RPCContext>
    {
        private static RPCFriendsApiBridge i;

        private readonly IRPC rpc;
        private readonly IFriendsApiBridge fallbackApiBridge;

        public event Action<FriendshipInitializationMessage> OnInitialized
        {
            add => fallbackApiBridge.OnInitialized += value;
            remove => fallbackApiBridge.OnInitialized -= value;
        }

        public event Action<string> OnFriendNotFound
        {
            add => fallbackApiBridge.OnFriendNotFound += value;
            remove => fallbackApiBridge.OnFriendNotFound -= value;
        }

        public event Action<AddFriendsPayload> OnFriendsAdded
        {
            add => fallbackApiBridge.OnFriendsAdded += value;
            remove => fallbackApiBridge.OnFriendsAdded -= value;
        }

        public event Action<AddFriendRequestsPayload> OnFriendRequestsAdded
        {
            add => fallbackApiBridge.OnFriendRequestsAdded += value;
            remove => fallbackApiBridge.OnFriendRequestsAdded -= value;
        }

        public event Action<AddFriendsWithDirectMessagesPayload> OnFriendWithDirectMessagesAdded
        {
            add => fallbackApiBridge.OnFriendWithDirectMessagesAdded += value;
            remove => fallbackApiBridge.OnFriendWithDirectMessagesAdded -= value;
        }

        public event Action<UserStatus> OnUserPresenceUpdated
        {
            add => fallbackApiBridge.OnUserPresenceUpdated += value;
            remove => fallbackApiBridge.OnUserPresenceUpdated -= value;
        }

        public event Action<FriendshipUpdateStatusMessage> OnFriendshipStatusUpdated;

        public event Action<UpdateTotalFriendRequestsPayload> OnTotalFriendRequestCountUpdated
        {
            add => fallbackApiBridge.OnTotalFriendRequestCountUpdated += value;
            remove => fallbackApiBridge.OnTotalFriendRequestCountUpdated -= value;
        }

        public event Action<UpdateTotalFriendsPayload> OnTotalFriendCountUpdated
        {
            add => fallbackApiBridge.OnTotalFriendCountUpdated += value;
            remove => fallbackApiBridge.OnTotalFriendCountUpdated -= value;
        }

        public event Action<FriendRequestPayload> OnFriendRequestReceived;

        public static RPCFriendsApiBridge CreateSharedInstance(IRPC rpc, IFriendsApiBridge fallbackApiBridge)
        {
            i = new RPCFriendsApiBridge(rpc, fallbackApiBridge);
            return i;
        }

        public static void RegisterService(RpcServerPort<RPCContext> port)
        {
            FriendRequestRendererServiceCodeGen.RegisterService(port, i);
        }

        public RPCFriendsApiBridge(IRPC rpc, IFriendsApiBridge fallbackApiBridge)
        {
            this.rpc = rpc;
            this.fallbackApiBridge = fallbackApiBridge;
        }

        public void RejectFriendship(string userId) =>
            fallbackApiBridge.RejectFriendship(userId);

        public async UniTask<RejectFriendshipPayload> RejectFriendshipAsync(string friendRequestId, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // TODO: pass cancellation token to rpc client when is supported
                KernelRejectFriendRequestReply response = await rpc.FriendRequests()
                                                                   .RejectFriendRequest(new KernelRejectFriendRequestPayload
                                                                    {
                                                                        FriendRequestId = friendRequestId
                                                                    });

                cancellationToken.ThrowIfCancellationRequested();

                RejectFriendshipPayload payload = response.MessageCase == KernelRejectFriendRequestReply.MessageOneofCase.Reply
                    ? new RejectFriendshipPayload
                    {
                        FriendRequestPayload = ToFriendRequestPayload(response.Reply.FriendRequest),
                    }
                    : throw new FriendshipException(ToErrorCode(response.Error));

                await UniTask.SwitchToMainThread(cancellationToken);

                return payload;
            }
            catch (Exception)
            {
                await UniTask.SwitchToMainThread();
                throw;
            }
        }

        public void RemoveFriend(string userId) =>
            fallbackApiBridge.RemoveFriend(userId);

        public void GetFriends(int limit, int skip) =>
            fallbackApiBridge.GetFriends(limit, skip);

        public void GetFriends(string usernameOrId, int limit) =>
            fallbackApiBridge.GetFriends(usernameOrId, limit);

        public void GetFriendRequests(int sentLimit, int sentSkip, int receivedLimit, int receivedSkip) =>
            fallbackApiBridge.GetFriendRequests(sentLimit, sentSkip, receivedLimit, receivedSkip);

        public async UniTask<AddFriendRequestsV2Payload> GetFriendRequestsAsync(int sentLimit, int sentSkip,
            int receivedLimit, int receivedSkip,
            CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // TODO: pass cancellation token to rpc client when is supported
                GetFriendRequestsReply response = await rpc.FriendRequests()
                                                           .GetFriendRequests(new KernelGetFriendRequestsPayload
                                                            {
                                                                SentLimit = sentLimit,
                                                                SentSkip = sentSkip,
                                                                ReceivedLimit = receivedLimit,
                                                                ReceivedSkip = receivedSkip
                                                            });

                cancellationToken.ThrowIfCancellationRequested();

                AddFriendRequestsV2Payload payload = response.MessageCase == GetFriendRequestsReply.MessageOneofCase.Error
                    ? throw new FriendshipException(ToErrorCode(response.Error))
                    : new AddFriendRequestsV2Payload
                    {
                        requestedTo = response.Reply.RequestedTo.Select(ToFriendRequestPayload).ToArray(),
                        requestedFrom = response.Reply.RequestedFrom.Select(ToFriendRequestPayload).ToArray(),
                        totalReceivedFriendRequests = response.Reply.TotalReceivedFriendRequests,
                        totalSentFriendRequests = response.Reply.TotalSentFriendRequests,
                    };

                await UniTask.SwitchToMainThread(cancellationToken);

                return payload;
            }
            catch (Exception)
            {
                await UniTask.SwitchToMainThread();
                throw;
            }
        }

        public void GetFriendsWithDirectMessages(string usernameOrId, int limit, int skip) =>
            fallbackApiBridge.GetFriendsWithDirectMessages(usernameOrId, limit, skip);

        public void RequestFriendship(string friendUserId) =>
            fallbackApiBridge.RequestFriendship(friendUserId);

        public async UniTask<RequestFriendshipConfirmationPayload> RequestFriendshipAsync(string userId, string messageBody,
            CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // TODO: pass cancellation token to rpc client when is supported
                SendFriendRequestReply reply = await rpc.FriendRequests()
                                                        .SendFriendRequest(new SendFriendRequestPayload
                                                         {
                                                             MessageBody = messageBody,
                                                             UserId = userId,
                                                         });

                cancellationToken.ThrowIfCancellationRequested();

                RequestFriendshipConfirmationPayload payload = reply.MessageCase == SendFriendRequestReply.MessageOneofCase.Reply
                    ? new RequestFriendshipConfirmationPayload
                    {
                        friendRequest = ToFriendRequestPayload(reply.Reply.FriendRequest),
                    }
                    : throw new FriendshipException(ToErrorCode(reply.Error));

                await UniTask.SwitchToMainThread(cancellationToken);

                return payload;
            }
            catch (Exception)
            {
                await UniTask.SwitchToMainThread();
                throw;
            }
        }

        public async UniTask<CancelFriendshipConfirmationPayload> CancelRequestAsync(string friendRequestId,
            CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // TODO: pass cancellation token to rpc client when is supported
                KernelCancelFriendRequestReply reply = await rpc.FriendRequests()
                                                                .CancelFriendRequest(new KernelCancelFriendRequestPayload
                                                                 {
                                                                     FriendRequestId = friendRequestId
                                                                 });

                cancellationToken.ThrowIfCancellationRequested();

                CancelFriendshipConfirmationPayload payload = reply.MessageCase == KernelCancelFriendRequestReply.MessageOneofCase.Reply
                    ? new CancelFriendshipConfirmationPayload
                    {
                        friendRequest = ToFriendRequestPayload(reply.Reply.FriendRequest),
                    }
                    : throw new FriendshipException(ToErrorCode(reply.Error));

                await UniTask.SwitchToMainThread(cancellationToken);

                return payload;
            }
            catch (Exception)
            {
                await UniTask.SwitchToMainThread(cancellationToken);
                throw;
            }
        }

        public UniTask CancelRequestByUserIdAsync(string userId, CancellationToken cancellationToken) =>
            fallbackApiBridge.CancelRequestByUserIdAsync(userId, cancellationToken);

        public void CancelRequestByUserId(string userId) =>
            fallbackApiBridge.CancelRequestByUserId(userId);

        public void AcceptFriendship(string userId) =>
            fallbackApiBridge.AcceptFriendship(userId);

        [PublicAPI]
        public async UniTask<ApproveFriendRequestReply> ApproveFriendRequest(ApproveFriendRequestPayload request, RPCContext context, CancellationToken ct)
        {
            await UniTask.SwitchToMainThread(ct);

            OnFriendshipStatusUpdated?.Invoke(new FriendshipUpdateStatusMessage
            {
                action = FriendshipAction.APPROVED,
                userId = request.UserId
            });

            return new ApproveFriendRequestReply();
        }

        [PublicAPI]
        public async UniTask<RendererRejectFriendRequestReply> RejectFriendRequest(RendererRejectFriendRequestPayload request, RPCContext context, CancellationToken ct)
        {
            await UniTask.SwitchToMainThread(ct);

            OnFriendshipStatusUpdated?.Invoke(new FriendshipUpdateStatusMessage
            {
                action = FriendshipAction.REJECTED,
                userId = request.UserId
            });

            return new RendererRejectFriendRequestReply();
        }

        [PublicAPI]
        public async UniTask<RendererCancelFriendRequestReply> CancelFriendRequest(RendererCancelFriendRequestPayload request, RPCContext context, CancellationToken ct)
        {
            await UniTask.SwitchToMainThread(ct);

            OnFriendshipStatusUpdated?.Invoke(new FriendshipUpdateStatusMessage
            {
                action = FriendshipAction.CANCELLED,
                userId = request.UserId
            });

            return new RendererCancelFriendRequestReply();
        }

        [PublicAPI]
        public async UniTask<ReceiveFriendRequestReply> ReceiveFriendRequest(ReceiveFriendRequestPayload request, RPCContext context, CancellationToken ct)
        {
            await UniTask.SwitchToMainThread(ct);

            OnFriendRequestReceived?.Invoke(ToFriendRequestPayload(request.FriendRequest));

            OnFriendshipStatusUpdated?.Invoke(new FriendshipUpdateStatusMessage
            {
                action = FriendshipAction.REQUESTED_FROM,
                userId = request.FriendRequest.From
            });

            return new ReceiveFriendRequestReply();
        }

        public async UniTask<AcceptFriendshipPayload> AcceptFriendshipAsync(string friendRequestId, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // TODO: pass cancellation token to rpc client when is supported
                AcceptFriendRequestReply response = await rpc.FriendRequests()
                                                             .AcceptFriendRequest(new AcceptFriendRequestPayload
                                                              {
                                                                  FriendRequestId = friendRequestId
                                                              });

                cancellationToken.ThrowIfCancellationRequested();

                AcceptFriendshipPayload payload = response.MessageCase == AcceptFriendRequestReply.MessageOneofCase.Reply
                    ? new AcceptFriendshipPayload
                    {
                        FriendRequest = ToFriendRequestPayload(response.Reply.FriendRequest)
                    }
                    : throw new FriendshipException(ToErrorCode(response.Error));

                await UniTask.SwitchToMainThread(cancellationToken);

                return payload;
            }
            catch (Exception)
            {
                await UniTask.SwitchToMainThread();
                throw;
            }
        }

        public async UniTask<FriendshipStatus> GetFriendshipStatus(string userId, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // TODO: pass cancellation token to rpc client when is supported
                GetFriendshipStatusResponse response = await rpc.Friends()
                                                                .GetFriendshipStatus(new GetFriendshipStatusRequest
                                                                 {
                                                                     UserId = userId
                                                                 });

                cancellationToken.ThrowIfCancellationRequested();

                FriendshipStatus status = ToFriendshipStatus(response.Status);

                await UniTask.SwitchToMainThread(cancellationToken);

                return status;
            }
            catch (Exception)
            {
                await UniTask.SwitchToMainThread();
                throw;
            }
        }

        private FriendshipStatus ToFriendshipStatus(RPCFriendshipStatus status)
        {
            switch (status)
            {
                case RPCFriendshipStatus.Approved:
                    return FriendshipStatus.FRIEND;
                case RPCFriendshipStatus.None:
                default:
                    return FriendshipStatus.NOT_FRIEND;
                case RPCFriendshipStatus.RequestedFrom:
                    return FriendshipStatus.REQUESTED_FROM;
                case RPCFriendshipStatus.RequestedTo:
                    return FriendshipStatus.REQUESTED_TO;
            }
        }

        private static FriendRequestPayload ToFriendRequestPayload(FriendRequestInfo request) =>
            new()
            {
                from = request.From,
                timestamp = (long)request.Timestamp,
                to = request.To,
                messageBody = request.MessageBody,
                friendRequestId = request.FriendRequestId
            };

        private FriendRequestErrorCodes ToErrorCode(FriendshipErrorCode code)
        {
            switch (code)
            {
                default:
                case FriendshipErrorCode.FecUnknown:
                    return FriendRequestErrorCodes.Unknown;
                case FriendshipErrorCode.FecBlockedUser:
                    return FriendRequestErrorCodes.BlockedUser;
                case FriendshipErrorCode.FecInvalidRequest:
                    return FriendRequestErrorCodes.InvalidRequest;
                case FriendshipErrorCode.FecNonExistingUser:
                    return FriendRequestErrorCodes.NonExistingUser;
                case FriendshipErrorCode.FecNotEnoughTimePassed:
                    return FriendRequestErrorCodes.NotEnoughTimePassed;
                case FriendshipErrorCode.FecTooManyRequestsSent:
                    return FriendRequestErrorCodes.TooManyRequestsSent;
            }
        }
    }
}
