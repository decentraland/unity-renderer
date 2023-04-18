using Cysharp.Threading.Tasks;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapLayers.Atlas;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace DCLServices.MapRendererV2.Tests
{
    [TestFixture]
    [Category("EditModeCI")]
    public class ChunkAtlasControllerShould
    {
        private const int PARCEL_SIZE = 20;
        private const int CHUNK_SIZE = 1000;
        private const int FRAME_DELAY = 5;

        private ChunkAtlasController atlasController;
        private ChunkAtlasController.ChunkBuilder builder;
        private int iterationsNumber;

        [SetUp]
        public void Setup()
        {
            var coordUtils = Substitute.For<ICoordsUtils>();
            coordUtils.ParcelSize.Returns(PARCEL_SIZE);
            coordUtils.WorldMinCoords.Returns(new Vector2Int(-150, -150));
            coordUtils.WorldMaxCoords.Returns(new Vector2Int(175, 175));

            builder = Substitute.For<ChunkAtlasController.ChunkBuilder>();

            atlasController = new ChunkAtlasController(null, null, CHUNK_SIZE, coordUtils, Substitute.For<IMapCullingController>(), builder);

            var parcelsInsideChunk = CHUNK_SIZE / PARCEL_SIZE;

            iterationsNumber =
                Mathf.CeilToInt((coordUtils.WorldMaxCoords.x - coordUtils.WorldMinCoords.x) / (float)parcelsInsideChunk)
                * Mathf.CeilToInt((coordUtils.WorldMaxCoords.y - coordUtils.WorldMinCoords.y) / (float)parcelsInsideChunk);
        }

        [Test]
        public async Task PerformsCorrectNumberOfIterations()
        {
            builder.Invoke(Arg.Any<Vector3>(), Arg.Any<Vector2Int>(), Arg.Any<Transform>(), Arg.Any<CancellationToken>())
                   .Returns(_ => UniTask.DelayFrame(FRAME_DELAY).ContinueWith(() => Substitute.For<IChunkController>()));

            await atlasController.Initialize(CancellationToken.None);

            builder.Received(iterationsNumber);
        }

        [Test]
        public async Task CreateChunksInBatches()
        {
            var invocationFrames = new List<int>();

            int frameNumber = 0;

            void CountEditorFrames()
            {
                frameNumber++;
            }

            EditorApplication.update += CountEditorFrames;

            builder.Invoke(Arg.Any<Vector3>(), Arg.Any<Vector2Int>(), Arg.Any<Transform>(), Arg.Any<CancellationToken>())
                   .Returns(_ =>
                        UniTask.DelayFrame(FRAME_DELAY)
                               .ContinueWith(() => invocationFrames.Add(frameNumber))
                               .ContinueWith(() => Substitute.For<IChunkController>()));

            // -1 corresponds to special logic in UniTask that subtracts one frame in Editor
            var frame = FRAME_DELAY - 1;

            var expected = new int[iterationsNumber];

            for (var i = 0; i < iterationsNumber; i++)
                expected[i] = frame + i / ChunkAtlasController.CHUNKS_CREATED_PER_BATCH * FRAME_DELAY;

            await atlasController.Initialize(CancellationToken.None);

            EditorApplication.update -= CountEditorFrames;

            CollectionAssert.AreEqual(expected, invocationFrames);
        }
    }
}
