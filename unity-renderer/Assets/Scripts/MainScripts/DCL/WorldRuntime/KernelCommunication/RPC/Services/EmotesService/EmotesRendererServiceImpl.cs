using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Controllers;
using DCL.Emotes;
using Decentraland.Renderer.RendererServices;
using rpc_csharp;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Environment = DCL.Environment;

namespace RPC.Services
{
    public class EmotesRendererServiceImpl : IEmotesRendererService<RPCContext>
    {
        private static readonly SuccessResponse FAILURE_RESPONSE = new SuccessResponse() { Success = false };
        private static readonly SuccessResponse SUCCESS_RESPONSE = new SuccessResponse() { Success = true };

        private readonly IWorldState worldState;
        private readonly ISceneController sceneController;
        private readonly UserProfile userProfile;
        private readonly BaseVariable<Player> ownPlayer;
        private readonly IBaseDictionary<(string bodyshapeId, string emoteId), EmoteClipData> alreadyLoadedEmotes;
        private readonly BaseRefCountedCollection<(string bodyshapeId, string emoteId)> emotesInUse;
        private readonly IDictionary<IParcelScene, HashSet<(string bodyshapeId, string emoteId)>> pendingEmotesByScene;
        private readonly IDictionary<IParcelScene, HashSet<(string bodyshapeId, string emoteId)>> equippedEmotesByScene;
        private readonly IDictionary<IParcelScene, CancellationTokenSource> cancellationTokenSources;

        private IAvatar avatarData;

        public static void RegisterService(RpcServerPort<RPCContext> port)
        {
            EmotesRendererServiceCodeGen.RegisterService(
                port,
                new EmotesRendererServiceImpl(
                    port: port,
                    worldState: Environment.i.world.state,
                    sceneController: Environment.i.world.sceneController,
                    userProfile: UserProfile.GetOwnUserProfile(),
                    ownPlayer: DataStore.i.player.ownPlayer,
                    alreadyLoadedEmotes: DataStore.i.emotes.animations,
                    emotesInUse: DataStore.i.emotes.emotesOnUse,
                    pendingEmotesByScene: new Dictionary<IParcelScene, HashSet<(string bodyshapeId, string emoteId)>>(),
                    equippedEmotesByScene: new Dictionary<IParcelScene, HashSet<(string bodyshapeId, string emoteId)>>(),
                    cancellationTokenSources: new Dictionary<IParcelScene, CancellationTokenSource>()
                ));
        }

        public EmotesRendererServiceImpl(
            RpcServerPort<RPCContext> port,
            IWorldState worldState,
            ISceneController sceneController,
            UserProfile userProfile,
            BaseVariable<Player> ownPlayer,
            IBaseDictionary<(string bodyshapeId, string emoteId), EmoteClipData> alreadyLoadedEmotes,
            BaseRefCountedCollection<(string bodyshapeId, string emoteId)> emotesInUse,
            IDictionary<IParcelScene, HashSet<(string bodyshapeId, string emoteId)>> pendingEmotesByScene,
            IDictionary<IParcelScene, HashSet<(string bodyshapeId, string emoteId)>> equippedEmotesByScene,
            IDictionary<IParcelScene, CancellationTokenSource> cancellationTokenSources
        )
        {
            this.worldState = worldState;
            this.sceneController = sceneController;
            this.userProfile = userProfile;
            this.ownPlayer = ownPlayer;
            this.alreadyLoadedEmotes = alreadyLoadedEmotes;
            this.pendingEmotesByScene = pendingEmotesByScene;
            this.equippedEmotesByScene = equippedEmotesByScene;
            this.emotesInUse = emotesInUse;
            this.cancellationTokenSources = cancellationTokenSources;

            port.OnClose += OnPortClosed;
        }

        private void OnPortClosed()
        {
            foreach (var scenes in worldState.GetLoadedScenes())
            {
                OnSceneRemoved(scenes.Value);
            }
        }

        public async UniTask<TriggerSelfUserExpressionResponse> TriggerSelfUserExpression(TriggerSelfUserExpressionRequest request, RPCContext context, CancellationToken ct)
        {
            await UniTask.SwitchToMainThread(ct);
            userProfile.SetAvatarExpression(request.Id, UserProfile.EmoteSource.Command);
            return default;
        }

        public async UniTask<SuccessResponse> TriggerSceneExpression(TriggerSceneExpressionRequest request, RPCContext context, CancellationToken ct)
        {
            // try get loaded scene
            if (!worldState.TryGetScene(request.SceneNumber, out IParcelScene scene))
                return FAILURE_RESPONSE;

            // generates an emote id
            if (!SceneEmoteHelper.TryGenerateEmoteId(scene, request.Path, request.Loop, out string emoteId))
                return FAILURE_RESPONSE;

            avatarData ??= ownPlayer.Get()?.avatar;

            if (avatarData == null)
                return FAILURE_RESPONSE;

            string userBodyShape = userProfile.avatar.bodyShape;

            // get hashset for scene emotes that are currently loading or create one if none
            if (!pendingEmotesByScene.TryGetValue(scene, out HashSet<(string bodyshapeId, string emoteId)> scenePendingEmotes))
            {
                scenePendingEmotes = new HashSet<(string bodyshapeId, string emoteId)>();
                pendingEmotesByScene.Add(scene, scenePendingEmotes);
            }

            // get hashset for scene emotes that are already equipped
            if (!equippedEmotesByScene.TryGetValue(scene, out HashSet<(string bodyshapeId, string emoteId)> sceneEquippedEmotes))
            {
                sceneEquippedEmotes = new HashSet<(string bodyshapeId, string emoteId)>();
                equippedEmotesByScene.Add(scene, sceneEquippedEmotes);
            }

            // get / create cancellation source for scene
            if (!cancellationTokenSources.TryGetValue(scene, out var cancellationTokenSource))
            {
                cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cancellationTokenSources.Add(scene, cancellationTokenSource);
            }

            sceneController.OnSceneRemoved -= OnSceneRemoved;
            sceneController.OnSceneRemoved += OnSceneRemoved;

            try
            {
                await UniTask.SwitchToMainThread(ct);

                // request emote to load using DataStore::emotes which will make `EmoteAnimationsTracker` plugin to load the emote
                await SceneEmoteHelper.RequestLoadSceneEmote(
                    userBodyShape,
                    emoteId,
                    alreadyLoadedEmotes,
                    emotesInUse,
                    scenePendingEmotes,
                    sceneEquippedEmotes,
                    cancellationTokenSource.Token);

                // make emote play
                avatarData.EquipEmote(emoteId, alreadyLoadedEmotes[(userBodyShape, emoteId)]);

                userProfile.SetAvatarExpression(emoteId, UserProfile.EmoteSource.Command);
                return SUCCESS_RESPONSE;
            }
            catch (OperationCanceledException _)
            {
                return FAILURE_RESPONSE;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return FAILURE_RESPONSE;
            }
        }

        private void OnSceneRemoved(IParcelScene scene)
        {
            if (cancellationTokenSources.TryGetValue(scene, out var cancellationTokenSource))
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSources.Remove(scene);
            }

            if (equippedEmotesByScene.TryGetValue(scene, out var equippedEmotes))
            {
                foreach (var emoteData in equippedEmotes)
                {
                    avatarData?.UnequipEmote(emoteData.emoteId);
                    emotesInUse.DecreaseRefCount(emoteData);
                }

                equippedEmotesByScene.Remove(scene);
            }

            if (pendingEmotesByScene.TryGetValue(scene, out var pendingEmotes))
            {
                foreach (var emoteData in pendingEmotes)
                {
                    emotesInUse.DecreaseRefCount((bodyshapeId: emoteData.bodyshapeId, emoteId: emoteData.emoteId));
                }

                pendingEmotesByScene.Remove(scene);
            }
        }
    }
}
