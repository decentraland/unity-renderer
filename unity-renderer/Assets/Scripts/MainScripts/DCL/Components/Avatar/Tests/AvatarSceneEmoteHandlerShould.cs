using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Emotes;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    public class AvatarSceneEmoteHandlerShould
    {
        private AvatarSceneEmoteHandler handler;
        private IAvatarEmotesController emotesController;
        private IEmotesService emotesService;

        [SetUp]
        public void SetUp()
        {
            emotesService = Substitute.For<IEmotesService>();
            emotesController = Substitute.For<IAvatarEmotesController>();
            handler = new AvatarSceneEmoteHandler(emotesController, emotesService);
        }

        [Test]
        public async Task LoadAndPlayEmote_Success()
        {
            const string BODY_SHAPE = "nogender";
            const string EMOTE_ID = "emoteId";

            var emote = SetupEmoteService(BODY_SHAPE, EMOTE_ID);

            await handler.LoadAndPlayEmote(BODY_SHAPE, EMOTE_ID);

            var emoteData = (bodyshapeId: BODY_SHAPE, emoteId: EMOTE_ID);
            emotesService.Received(1).RequestEmote(emoteData.bodyshapeId, emoteData.emoteId, Arg.Any<CancellationToken>());
            emotesController.Received(1).EquipEmote(EMOTE_ID, emote);
            emotesController.Received(1).PlayEmote(EMOTE_ID, 0);
        }

        [Test]
        public async Task LoadAndPlayEmote_Cancelled()
        {
            const string BODY_SHAPE = "nogender";
            const string EMOTE_ID = "emoteId";

            emotesService.RequestEmote(BODY_SHAPE, EMOTE_ID, Arg.Any<CancellationToken>()).Throws(new OperationCanceledException());

            await handler.LoadAndPlayEmote(BODY_SHAPE, EMOTE_ID);

            var emoteData = (bodyshapeId: BODY_SHAPE, emoteId: EMOTE_ID);
            emotesService.Received(1).RequestEmote(emoteData.bodyshapeId, emoteData.emoteId, Arg.Any<CancellationToken>());
            emotesController.DidNotReceive().EquipEmote(Arg.Any<string>(), Arg.Any<IEmoteReference>());
            emotesController.DidNotReceive().PlayEmote(Arg.Any<string>(), Arg.Any<long>());
        }

        [Test]
        public async Task LoadAndPlayEmote_LoadTwice()
        {
            const string BODY_SHAPE = "nogender";
            const string EMOTE_ID1 = "emoteId";
            const string EMOTE_ID2 = "emoteId2";

            var emote1 = SetupEmoteService(BODY_SHAPE, EMOTE_ID1);
            var emote2 = SetupEmoteService(BODY_SHAPE, EMOTE_ID2);

            handler.SetExpressionLamportTimestamp(0);
            var load1 = handler.LoadAndPlayEmote(BODY_SHAPE, EMOTE_ID1);
            handler.SetExpressionLamportTimestamp(1);
            var load2 = handler.LoadAndPlayEmote(BODY_SHAPE, EMOTE_ID2);

            await UniTask.WhenAll(load1, load2);

            // first emote is equipped
            emotesController.Received(1).EquipEmote(EMOTE_ID1, emote1);
            // but not played
            emotesController.DidNotReceive().PlayEmote(EMOTE_ID1, 0);
            // second emote is equipped
            emotesController.Received(1).EquipEmote(EMOTE_ID2, emote2);
            // and played
            emotesController.Received(1).PlayEmote(EMOTE_ID2, 1);
        }

        [Test]
        public async Task CleanUp()
        {
            const string BODY_SHAPE = "nogender";
            const string EMOTE_ID = "emoteId";

            var emote = SetupEmoteService(BODY_SHAPE, EMOTE_ID);

            await handler.LoadAndPlayEmote(BODY_SHAPE, EMOTE_ID);

            emotesController.Received(1).EquipEmote(EMOTE_ID, emote);
            emotesController.Received(1).PlayEmote(EMOTE_ID, 0);

            handler.CleanUp();

            emotesController.Received(1).UnEquipEmote(EMOTE_ID);
        }

        private IEmoteReference SetupEmoteService(string bodyShape, string emoteId)
        {
            var emote = Substitute.For<IEmoteReference>();
            emotesService.RequestEmote(bodyShape, emoteId, Arg.Any<CancellationToken>()).Returns(_ => GetResult(emote));
            return emote;
        }

        // the yield hack was used for the test LoadAndPlayEmote_LoadTwice in order to simulate that the emotes are not loaded yet
        private async UniTask<IEmoteReference> GetResult(IEmoteReference emote)
        {
            await UniTask.Yield();
            return emote;
        }
    }
}
