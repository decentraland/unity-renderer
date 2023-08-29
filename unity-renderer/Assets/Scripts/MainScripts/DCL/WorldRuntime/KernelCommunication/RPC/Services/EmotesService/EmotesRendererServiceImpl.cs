using AvatarAssets;
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
        private static readonly SuccessResponse FAILURE_RESPONSE = new () { Success = false };
        private static readonly SuccessResponse SUCCESS_RESPONSE = new () { Success = true };

        private readonly IWorldState worldState;
        private readonly ISceneController sceneController;
        private readonly UserProfile userProfile;
        private readonly BaseVariable<Player> ownPlayer;
        private readonly IEmotesService emotesService;

        private readonly IDictionary<IParcelScene, HashSet<(string bodyShape, string emoteId)>> emotesByScene;
        private readonly IDictionary<IParcelScene, CancellationTokenSource> cancellationTokenSources;

        private IAvatarEmotesController emotesController;

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
                    emotesService: Environment.i.serviceLocator.Get<IEmotesService>(),
                    emotesByScene: new Dictionary<IParcelScene, HashSet<(string bodyShape, string emoteId)>>(),
                    cancellationTokenSources: new Dictionary<IParcelScene, CancellationTokenSource>()
                ));
        }

        public EmotesRendererServiceImpl(
            RpcServerPort<RPCContext> port,
            IWorldState worldState,
            ISceneController sceneController,
            UserProfile userProfile,
            BaseVariable<Player> ownPlayer,
            IEmotesService emotesService,
            IDictionary<IParcelScene, HashSet<(string bodyShape, string emoteId)>> emotesByScene,
            IDictionary<IParcelScene, CancellationTokenSource> cancellationTokenSources
        )
        {
            this.worldState = worldState;
            this.sceneController = sceneController;
            this.userProfile = userProfile;
            this.ownPlayer = ownPlayer;
            this.emotesService = emotesService;
            this.emotesByScene = emotesByScene;
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

            emotesController ??= ownPlayer.Get()?.avatar.GetEmotesController();

            if (emotesController == null)
                return FAILURE_RESPONSE;

            string userBodyShape = userProfile.avatar.bodyShape;

            // get hashset for scene emotes that are already equipped
            if (!emotesByScene.TryGetValue(scene, out HashSet<(string bodyShape, string emoteId)> sceneEquippedEmotes))
            {
                sceneEquippedEmotes = new HashSet<(string bodyShape, string emoteId)>();
                emotesByScene.Add(scene, sceneEquippedEmotes);
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

                if (!emotesByScene[scene].Contains((userBodyShape, emoteId)))
                {
                    var result = await emotesService.RequestEmote(userBodyShape, emoteId, cancellationTokenSource.Token);
                    emotesController.EquipEmote(emoteId, result);
                    emotesByScene[scene].Add((userBodyShape, emoteId));
                }

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

            if (!emotesByScene.TryGetValue(scene, out var equippedEmotes)) return;

            foreach (var emoteData in equippedEmotes)
                emotesController?.UnEquipEmote(emoteData.emoteId);

            emotesByScene.Remove(scene);
        }
    }
}
