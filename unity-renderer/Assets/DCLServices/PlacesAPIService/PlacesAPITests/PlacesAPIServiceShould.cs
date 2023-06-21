using Cysharp.Threading.Tasks;
using DCLServices.Lambdas;
using MainScripts.DCL.Controllers.HotScenes;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace DCLServices.PlacesAPIService.PlacesAPITests
{
    public class PlacesAPIServiceShould
    {
        private IPlacesAPIClient client;
        private PlacesAPIService service;
        private Dictionary<string, IHotScenesController.PlaceInfo> placesCatalog;

        [SetUp]
        public void SetUp()
        {
            PreparePlacesCatalog();
            client = Substitute.For<IPlacesAPIClient>();
            service = new PlacesAPIService(client);
        }

        [TestCase(0, 3)]
        [TestCase(1, 3)]
        [TestCase(2, 2)]
        [TestCase(3, 2)]
        public async Task GetMostActivePlacesWhenNoCached(int pageNumber, int pageSize)
        {
            // Arrange
            var wholeCatalog = placesCatalog.Values.ToList();

            client.GetMostActivePlaces(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                  .Returns((x) => new UniTask<IHotScenesController.PlacesAPIResponse>(new IHotScenesController.PlacesAPIResponse
                   {
                       ok = true,
                       total = x.ArgAt<int>(1),
                       data = wholeCatalog.Skip(x.ArgAt<int>(0) * x.ArgAt<int>(1)).Take(x.ArgAt<int>(1)).ToList(),
                   }));

            // Act
            var result = await service.GetMostActivePlaces(pageNumber, pageSize, default);

            // Assert
            client.Received().GetMostActivePlaces(pageNumber, pageSize, Arg.Any<CancellationToken>());
            Assert.AreEqual(pageSize, result.Count);
            for (int i = 0; i < pageSize; i++)
            {
                Assert.AreEqual(wholeCatalog[(pageNumber * pageSize) + i], result[i]);
            }
        }

        [TestCase(0,2)]
        [TestCase(1,2)]
        [TestCase(3,2)]
        [TestCase(1,3)]
        public async Task UsePageCacheForMostActivePlace(int pageNumber, int pageSize)
        {
            // Arrange
            var wholeCatalog = placesCatalog.Values.ToList();
            var pagePointer = new LambdaResponsePagePointer<IHotScenesController.PlacesAPIResponse>("/", 4, default, service);
            pagePointer.cachedPages.Add(pageNumber, new IHotScenesController.PlacesAPIResponse()
            {
                pageNum = pageNumber,
                pageSize = pageSize,
                ok = true,
                total = pageSize,
                totalAmount = pageSize,
                data = wholeCatalog.Skip(pageSize * pageNumber).Take(pageSize).ToList(),
            });
            service.activePlacesPagePointers.Add(pageSize, pagePointer);

            // Act
            var result = await service.GetMostActivePlaces(pageNumber, pageSize, default);

            // Assert
            client.DidNotReceiveWithAnyArgs().GetMostActivePlaces(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
            Assert.AreEqual(pageSize, result.Count);
            for (int i = 0; i < pageSize; i++)
            {
                Assert.AreEqual(wholeCatalog[(pageNumber * pageSize) + i], result[i]);
            }
        }

        [TestCase(0,2)]
        [TestCase(1,2)]
        [TestCase(3,2)]
        [TestCase(1,3)]
        public async Task DoNotUseCacheForMostActivePlaceWhenForced(int pageNumber, int pageSize)
        {
            // Arrange
            var wholeCatalog = placesCatalog.Values.ToList();
            client.GetMostActivePlaces(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                  .Returns((x) => new UniTask<IHotScenesController.PlacesAPIResponse>(new IHotScenesController.PlacesAPIResponse
                   {
                       ok = true,
                       total = x.ArgAt<int>(1),
                       data = wholeCatalog.Skip(x.ArgAt<int>(0) * x.ArgAt<int>(1)).Take(x.ArgAt<int>(1)).ToList(),
                   }));
            var pagePointer = new LambdaResponsePagePointer<IHotScenesController.PlacesAPIResponse>("/", 4, default, service);
            pagePointer.cachedPages.Add(pageNumber, new IHotScenesController.PlacesAPIResponse()
            {
                pageNum = pageNumber,
                pageSize = pageSize,
                ok = true,
                total = pageSize,
                totalAmount = pageSize,
                data = wholeCatalog.Skip(pageSize * pageNumber).Take(pageSize).ToList(),
            });
            service.activePlacesPagePointers.Add(pageSize, pagePointer);

            // Act
            var result = await service.GetMostActivePlaces(pageNumber, pageSize, default, true);

            // Assert
            client.Received().GetMostActivePlaces(pageNumber, pageSize, Arg.Any<CancellationToken>());
            Assert.AreEqual(pageSize, result.Count);
            for (int i = 0; i < pageSize; i++)
            {
                Assert.AreEqual(wholeCatalog[(pageNumber * pageSize) + i], result[i]);
            }
        }


        private void PreparePlacesCatalog()
        {
#region helper functions
            void AddPlaceInColumn(int column, int length)
            {
                Vector2Int[] positions = new Vector2Int[length];

                for (int i = 0; i < length; i++) { positions[i] = new Vector2Int(column, i); }

                var place = new IHotScenesController.PlaceInfo()
                {
                    id = $"{column}Column",
                    positions = positions
                };

                placesCatalog.Add(place.id, place);
            }

            void AddPlaceInSquare(Vector2Int basePosition, int squareSize)
            {
                Vector2Int[] positions = new Vector2Int[squareSize * squareSize];

                for (int i = 0; i < squareSize; i++)
                {
                    for (int j = 0; j < squareSize; j++) { positions[(i * squareSize) + j] = new Vector2Int(basePosition.x + i, basePosition.y + j); }
                }

                var place = new IHotScenesController.PlaceInfo()
                {
                    id = $"Square{basePosition}",
                    positions = positions
                };

                placesCatalog.Add(place.id, place);
            }

            void AddPlaceInfoSingle(Vector2Int basePosition)
            {
                var place = new IHotScenesController.PlaceInfo()
                {
                    id = $"{basePosition.x},{basePosition.y}",
                    positions = new[] { basePosition }
                };

                placesCatalog.Add(place.id, place);
            }
#endregion

            placesCatalog = new Dictionary<string, IHotScenesController.PlaceInfo>();

            // Places in column shape
            AddPlaceInColumn(0, 4);
            AddPlaceInColumn(1, 4);
            AddPlaceInColumn(2, 4);
            AddPlaceInColumn(3, 4);

            // Places in square shape
            AddPlaceInSquare(new Vector2Int(50, 50), 2);
            AddPlaceInSquare(new Vector2Int(-50, -50), 2);

            // Places with a single parcel
            AddPlaceInfoSingle(new Vector2Int(60, 60));
            AddPlaceInfoSingle(new Vector2Int(60, 61));
            AddPlaceInfoSingle(new Vector2Int(60, 62));
            AddPlaceInfoSingle(new Vector2Int(60, 63));
            AddPlaceInfoSingle(new Vector2Int(60, 64));
            AddPlaceInfoSingle(new Vector2Int(60, 65));
            AddPlaceInfoSingle(new Vector2Int(60, 66));
            AddPlaceInfoSingle(new Vector2Int(60, 67));
            AddPlaceInfoSingle(new Vector2Int(60, 68));
            AddPlaceInfoSingle(new Vector2Int(60, 69));
            AddPlaceInfoSingle(new Vector2Int(60, 70));
        }
    }
}
