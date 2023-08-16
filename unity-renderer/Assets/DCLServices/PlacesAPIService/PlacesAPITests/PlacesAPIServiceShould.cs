using Cysharp.Threading.Tasks;
using DCLServices.Lambdas;
using MainScripts.DCL.Controllers.HotScenes;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System;
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

            client.GetMostActivePlaces(Arg.Any<int>(), Arg.Any<int>(), "", "", Arg.Any<CancellationToken>())
                  .Returns((x) => new UniTask<IHotScenesController.PlacesAPIResponse>(new IHotScenesController.PlacesAPIResponse
                   {
                       ok = true,
                       total = x.ArgAt<int>(1),
                       data = wholeCatalog.Skip(x.ArgAt<int>(0) * x.ArgAt<int>(1)).Take(x.ArgAt<int>(1)).ToList(),
                   }));

            // Act
            var result = await service.GetMostActivePlaces(pageNumber, pageSize, default);

            // Assert
            client.Received().GetMostActivePlaces(pageNumber, pageSize, "", "", Arg.Any<CancellationToken>());
            Assert.AreEqual(pageSize, result.places.Count);
            for (int i = 0; i < pageSize; i++)
            {
                Assert.AreEqual(wholeCatalog[(pageNumber * pageSize) + i], result.places[i]);
            }
        }

        [Test]
        public async Task AddPlacesToCacheWhenCallingGetMostActivePlaces()
        {
            // Arrange
            client.GetMostActivePlaces(Arg.Any<int>(), Arg.Any<int>(), "", "", Arg.Any<CancellationToken>())
                  .Returns((x) => new UniTask<IHotScenesController.PlacesAPIResponse>(new IHotScenesController.PlacesAPIResponse
                   {
                       ok = true,
                       total = x.ArgAt<int>(1),
                       data = new List<IHotScenesController.PlaceInfo>()
                       {
                           placesCatalog["0Column"], //will add Positions {0,0} {0,1} {0,2} and {0,3}
                           placesCatalog["Square50,50"], //will add Positions {50,50} {50,51} {51,50} and {51,51}
                           placesCatalog["60,60"],
                       },
                   }));

            // Act
            await service.GetMostActivePlaces(0, 4, default);

            // Assert
            Assert.AreEqual(3, service.placesById.Count);
            Assert.AreEqual(9, service.placesByCoords.Count);

            Assert.AreEqual(placesCatalog["0Column"], service.placesById["0Column"]);
            Assert.AreEqual(placesCatalog["0Column"], service.placesByCoords[new Vector2Int(0,0)]);
            Assert.AreEqual(placesCatalog["0Column"], service.placesByCoords[new Vector2Int(0,1)]);
            Assert.AreEqual(placesCatalog["0Column"], service.placesByCoords[new Vector2Int(0,2)]);
            Assert.AreEqual(placesCatalog["0Column"], service.placesByCoords[new Vector2Int(0,3)]);

            Assert.AreEqual(placesCatalog["Square50,50"], service.placesById["Square50,50"]);
            Assert.AreEqual(placesCatalog["Square50,50"], service.placesByCoords[new Vector2Int(50,50)]);
            Assert.AreEqual(placesCatalog["Square50,50"], service.placesByCoords[new Vector2Int(51,50)]);
            Assert.AreEqual(placesCatalog["Square50,50"], service.placesByCoords[new Vector2Int(50,51)]);
            Assert.AreEqual(placesCatalog["Square50,50"], service.placesByCoords[new Vector2Int(51,51)]);

            Assert.AreEqual(placesCatalog["60,60"], service.placesById["60,60"]);
            Assert.AreEqual(placesCatalog["60,60"], service.placesByCoords[new Vector2Int(60,60)]);
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
            pagePointer.cachedPages.Add(pageNumber, (new IHotScenesController.PlacesAPIResponse()
            {
                pageNum = pageNumber,
                pageSize = pageSize,
                ok = true,
                total = pageSize,
                totalAmount = pageSize,
                data = wholeCatalog.Skip(pageSize * pageNumber).Take(pageSize).ToList(),
            }, DateTime.Now));
            service.activePlacesPagePointers.Add(pageSize.ToString(), pagePointer);

            // Act
            var result = await service.GetMostActivePlaces(pageNumber, pageSize, default);

            // Assert
            client.DidNotReceiveWithAnyArgs().GetMostActivePlaces(Arg.Any<int>(), Arg.Any<int>(),"", "", Arg.Any<CancellationToken>());
            Assert.AreEqual(pageSize, result.places.Count);
            for (int i = 0; i < pageSize; i++)
            {
                Assert.AreEqual(wholeCatalog[(pageNumber * pageSize) + i], result.places[i]);
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
            client.GetMostActivePlaces(Arg.Any<int>(), Arg.Any<int>(),"", "", Arg.Any<CancellationToken>())
                  .Returns((x) => new UniTask<IHotScenesController.PlacesAPIResponse>(new IHotScenesController.PlacesAPIResponse
                   {
                       ok = true,
                       total = x.ArgAt<int>(1),
                       data = wholeCatalog.Skip(x.ArgAt<int>(0) * x.ArgAt<int>(1)).Take(x.ArgAt<int>(1)).ToList(),
                   }));
            var pagePointer = new LambdaResponsePagePointer<IHotScenesController.PlacesAPIResponse>("/", 4, default, service);
            pagePointer.cachedPages.Add(pageNumber, (new IHotScenesController.PlacesAPIResponse()
            {
                pageNum = pageNumber,
                pageSize = pageSize,
                ok = true,
                total = pageSize,
                totalAmount = pageSize,
                data = wholeCatalog.Skip(pageSize * pageNumber).Take(pageSize).ToList(),
            }, DateTime.Now));
            service.activePlacesPagePointers.Add(pageSize.ToString(), pagePointer);

            // Act
            var result = await service.GetMostActivePlaces(pageNumber, pageSize, "", "", default, true);

            // Assert
            client.Received().GetMostActivePlaces(pageNumber, pageSize, "", "", Arg.Any<CancellationToken>());
            Assert.AreEqual(pageSize, result.places.Count);
            for (int i = 0; i < pageSize; i++)
            {
                Assert.AreEqual(wholeCatalog[(pageNumber * pageSize) + i], result.places[i]);
            }
        }

        [TestCase("60,65")]
        [TestCase("Square-50,-50")]
        [TestCase("2Column")]
        public async Task RetrievePlaceByIdIfNotCached(string id)
        {
            // Arrange
            client.GetPlace(Arg.Any<string>(), Arg.Any<CancellationToken>())
                  .Returns((x) => new UniTask<IHotScenesController.PlaceInfo>(placesCatalog[x.ArgAt<string>(0)]));

            // Act
            var result = await service.GetPlace(id, default);

            // Assert
            client.Received().GetPlace(id, Arg.Any<CancellationToken>());
            Assert.AreEqual(placesCatalog[id], result);
            Assert.AreEqual(1, service.placesById.Count);
            Assert.AreEqual(result, service.placesById[id]);
            Assert.AreEqual(result.Positions.Length, service.placesByCoords.Count);
            foreach (Vector2Int coords in result.Positions)
            {
                Assert.AreEqual(result, service.placesByCoords[coords]);
            }
        }

        [TestCase("60,65")]
        [TestCase("Square-50,-50")]
        [TestCase("2Column")]
        public async Task UseCacheInGetPlaceById(string id)
        {
            // Arrange
            service.placesById.Add(id, placesCatalog[id]);

            // Act
            var result = await service.GetPlace(id, default);

            // Assert
            client.DidNotReceive().GetPlace(id, Arg.Any<CancellationToken>());
            Assert.AreEqual(placesCatalog[id], result);
        }

        [TestCase("60,65")]
        [TestCase("Square-50,-50")]
        [TestCase("2Column")]
        public async Task DontUseCacheInGetPlaceByIdWhenRenewForced(string id)
        {
            // Arrange
            var newPlace = new IHotScenesController.PlaceInfo()
            {
                id = id,
                Positions = new []{new Vector2Int(0,0)},
            };
            client.Configure().GetPlace(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(x => new UniTask<IHotScenesController.PlaceInfo>(newPlace));
            service.placesById.Add(id, placesCatalog[id]);

            // Act
            var result = await service.GetPlace(id, default, true);

            // Assert
            client.Received().GetPlace(id, Arg.Any<CancellationToken>());
            Assert.AreEqual(newPlace, result);
            Assert.AreEqual(service.placesById[id], result);
        }


        public static IEnumerable<List<Vector2Int>> UseCacheInGetPlaceByCoords_1 { get { yield return new List<Vector2Int> { new (0, 0) }; } }
        public static IEnumerable<List<Vector2Int>> UseCacheInGetPlaceByCoords_2 { get { yield return new List<Vector2Int> { new (0, 0), new (1, 0), new (3, 0) }; } }
        public static IEnumerable<List<Vector2Int>> UseCacheInGetPlaceByCoords_3 { get { yield return new List<Vector2Int> { new (0, 0), new (0, 0), new (0, 0)  }; } }
        [Test]
        [TestCaseSource(nameof(UseCacheInGetPlaceByCoords_1))]
        [TestCaseSource(nameof(UseCacheInGetPlaceByCoords_2))]
        [TestCaseSource(nameof(UseCacheInGetPlaceByCoords_3))]
        public async Task UseCacheInGetPlaceByCoords(List<Vector2Int> coords)
        {
            // Arrange
            IHotScenesController.PlaceInfo place = new IHotScenesController.PlaceInfo()
            {
                id = "test",
                Positions = coords.ToArray(),
            };
            foreach (var coord in coords)
            {
                service.placesByCoords[coord] = place;
            }

            // Act & Assert
            foreach (var coord in coords)
            {
                var result = await service.GetPlace(coord, default);
                client.DidNotReceive().GetPlace(coord, Arg.Any<CancellationToken>());
                Assert.AreEqual(place, result);
            }
        }

        [Test]
        public async Task RetrieveFavoritesFromServer()
        {
            // Arrange
            var place0 = new IHotScenesController.PlaceInfo() { id = "test0", Positions = new[] { new Vector2Int(0, 0), new Vector2Int(0, 1) }, };
            var place1 = new IHotScenesController.PlaceInfo() { id = "test1", Positions = new[] { new Vector2Int(1, 0), new Vector2Int(1, 1) }, };
            client.GetFavorites(Arg.Any<CancellationToken>()).Returns(x => new UniTask<List<IHotScenesController.PlaceInfo>>(new List<IHotScenesController.PlaceInfo>() { place0, place1 }));

            // Act
            var result = await service.GetFavorites(default);

            // Assert
            Assert.AreEqual(place0, result[0]);
            Assert.AreEqual(place0, service.placesById[place0.id]);
            Assert.AreEqual(place0, service.placesByCoords[place0.Positions[0]]);
            Assert.AreEqual(place0, service.placesByCoords[place0.Positions[1]]);
            Assert.AreEqual(place1, result[1]);
            Assert.AreEqual(place1, service.placesById[place1.id]);
            Assert.AreEqual(place1, service.placesByCoords[place1.Positions[0]]);
            Assert.AreEqual(place1, service.placesByCoords[place1.Positions[1]]);
        }

        [Test]
        public async Task RetrieveCachedFavorites()
        {
            // Arrange
            var place0 = new IHotScenesController.PlaceInfo() { id = "test0", Positions = new[] { new Vector2Int(0, 0), new Vector2Int(0, 1) }, };
            var place1 = new IHotScenesController.PlaceInfo() { id = "test1", Positions = new[] { new Vector2Int(1, 0), new Vector2Int(1, 1) }, };
            service.CachePlace(place0);
            service.CachePlace(place1);
            service.serverFavoritesCompletionSource = new UniTaskCompletionSource<List<IHotScenesController.PlaceInfo>>();
            service.serverFavoritesCompletionSource.TrySetResult(new List<IHotScenesController.PlaceInfo>(){place0, place1});

            // Act
            var result = await service.GetFavorites(default);

            // Assert
            Assert.AreEqual(place0, result[0]);
            Assert.AreEqual(place0, service.placesById[place0.id]);
            Assert.AreEqual(place0, service.placesByCoords[place0.Positions[0]]);
            Assert.AreEqual(place0, service.placesByCoords[place0.Positions[1]]);
            Assert.AreEqual(place1, result[1]);
            Assert.AreEqual(place1, service.placesById[place1.id]);
            Assert.AreEqual(place1, service.placesByCoords[place1.Positions[0]]);
            Assert.AreEqual(place1, service.placesByCoords[place1.Positions[1]]);
        }

        [Test]
        public async Task RetrieveFavoritesFromServerWhenCacheIgnored()
        {
            // Arrange
            var place0 = new IHotScenesController.PlaceInfo() { id = "test0", Positions = new[] { new Vector2Int(0, 0), new Vector2Int(0, 1) }, description = "test0"};
            var place1 = new IHotScenesController.PlaceInfo() { id = "test1", Positions = new[] { new Vector2Int(1, 0), new Vector2Int(1, 1) }, description = "test1"};
            service.CachePlace(place0);
            service.CachePlace(place1);
            service.serverFavoritesCompletionSource = new UniTaskCompletionSource<List<IHotScenesController.PlaceInfo>>();
            service.serverFavoritesCompletionSource.TrySetResult(new List<IHotScenesController.PlaceInfo>(){place0, place1});
            var newPlace0 = new IHotScenesController.PlaceInfo() { id = "test0", Positions = new[] { new Vector2Int(0, 0), new Vector2Int(0, 1) }, description = "newTest0"};
            var newPlace1 = new IHotScenesController.PlaceInfo() { id = "test1", Positions = new[] { new Vector2Int(1, 0), new Vector2Int(1, 1) }, description = "newTest1"};
            client.GetFavorites(Arg.Any<CancellationToken>()).Returns(x => new UniTask<List<IHotScenesController.PlaceInfo>>(new List<IHotScenesController.PlaceInfo>() { newPlace0, newPlace1 }));

            // Act
            var result = await service.GetFavorites(default, true);

            // Assert
            client.Received().GetFavorites(Arg.Any<CancellationToken>());
            Assert.AreEqual(newPlace0, result[0]);
            Assert.AreEqual(newPlace0, service.placesById[newPlace0.id]);
            Assert.AreEqual(newPlace0, service.placesByCoords[newPlace0.Positions[0]]);
            Assert.AreEqual(newPlace0, service.placesByCoords[newPlace0.Positions[1]]);
            Assert.AreEqual("newTest0", result[0].description);
            Assert.AreEqual(newPlace1, result[1]);
            Assert.AreEqual(newPlace1, service.placesById[newPlace1.id]);
            Assert.AreEqual(newPlace1, service.placesByCoords[newPlace1.Positions[0]]);
            Assert.AreEqual(newPlace1, service.placesByCoords[newPlace1.Positions[1]]);
            Assert.AreEqual("newTest1", result[1].description);
        }

        [Test]
        public async Task RetrieveFavoritesIntoMultipleCalls()
        {
            // Arrange
            var place0 = new IHotScenesController.PlaceInfo() { id = "test0", Positions = new[] { new Vector2Int(0, 0), new Vector2Int(0, 1) }, };
            var place1 = new IHotScenesController.PlaceInfo() { id = "test1", Positions = new[] { new Vector2Int(1, 0), new Vector2Int(1, 1) }, };
            client.GetFavorites(Arg.Any<CancellationToken>()).Returns(x => new UniTask<List<IHotScenesController.PlaceInfo>>(new List<IHotScenesController.PlaceInfo>() { place0, place1 }));

            // Act
            (IReadOnlyList<IHotScenesController.PlaceInfo> firstCall, IReadOnlyList<IHotScenesController.PlaceInfo> secondCall) = await UniTask.WhenAll(service.GetFavorites(default), service.GetFavorites(default));

            // Assert
            Assert.AreEqual(place0,  firstCall[0]);
            Assert.AreEqual(place0,  secondCall[0]);
            Assert.AreEqual(place0, service.placesById[place0.id]);
            Assert.AreEqual(place0, service.placesByCoords[place0.Positions[0]]);
            Assert.AreEqual(place0, service.placesByCoords[place0.Positions[1]]);
            Assert.AreEqual(place1, firstCall[1]);
            Assert.AreEqual(place1, secondCall[1]);
            Assert.AreEqual(place1, service.placesById[place1.id]);
            Assert.AreEqual(place1, service.placesByCoords[place1.Positions[0]]);
            Assert.AreEqual(place1, service.placesByCoords[place1.Positions[1]]);
        }

        [Test]
        public async Task ComposeFavoritesProperly()
        {
            // Arrange
            service.serverFavoritesCompletionSource = new UniTaskCompletionSource<List<IHotScenesController.PlaceInfo>>();
            service.serverFavoritesCompletionSource.TrySetResult(new List<IHotScenesController.PlaceInfo>{ new() {title = "fromServer", description = "checkMe"}});
            service.CachePlace(new IHotScenesController.PlaceInfo(){title = "fromLocal", description = "checkMe"});
            service.localFavorites.Clear();
            service.localFavorites.Add("fromLocal", true);
            service.composedFavorites.Clear();
            service.composedFavorites.Add(new IHotScenesController.PlaceInfo{title = "oldCompose", description = "shouldntBeInResult"});
            service.composedFavoritesDirty = true;

            // Act
            var places = await service.GetFavorites(default);

            // Assert
            Assert.AreEqual(2, places.Count);
            Assert.IsTrue(places.All(x => x.description == "checkMe"));
            Assert.IsTrue(places.Any(x => x.title == "fromLocal"));
            Assert.IsTrue(places.Any(x => x.title == "fromServer"));
        }

        [Test]
        public async Task UseComposedFavoritesIfNotDirty()
        {
            // Arrange
            service.serverFavoritesCompletionSource = new UniTaskCompletionSource<List<IHotScenesController.PlaceInfo>>();
            service.serverFavoritesCompletionSource.TrySetResult(new List<IHotScenesController.PlaceInfo>{ new() {title = "thisShouldntBeInResult"}});
            service.composedFavoritesDirty = false;
            service.composedFavorites.Clear();
            service.composedFavorites.Add(new IHotScenesController.PlaceInfo{title = "checkMe"});

            // Act
            var places = await service.GetFavorites(default);

            // Assert
            Assert.AreEqual(1, places.Count);
            Assert.AreEqual("checkMe", places[0].title);
        }


        private void PreparePlacesCatalog()
        {
#region helper functions
            void AddPlaceInColumn(int column, int length)
            {
                Vector2Int[] Positions = new Vector2Int[length];

                for (int i = 0; i < length; i++) { Positions[i] = new Vector2Int(column, i); }

                var place = new IHotScenesController.PlaceInfo()
                {
                    id = $"{column}Column",
                    Positions = Positions
                };

                placesCatalog.Add(place.id, place);
            }

            void AddPlaceInSquare(Vector2Int basePosition, int squareSize)
            {
                Vector2Int[] Positions = new Vector2Int[squareSize * squareSize];

                for (int i = 0; i < squareSize; i++)
                {
                    for (int j = 0; j < squareSize; j++) { Positions[(i * squareSize) + j] = new Vector2Int(basePosition.x + i, basePosition.y + j); }
                }

                var place = new IHotScenesController.PlaceInfo()
                {
                    id = $"Square{basePosition.x},{basePosition.y}",
                    Positions = Positions
                };

                placesCatalog.Add(place.id, place);
            }

            void AddPlaceInfoSingle(Vector2Int basePosition)
            {
                var place = new IHotScenesController.PlaceInfo()
                {
                    id = $"{basePosition.x},{basePosition.y}",
                    Positions = new[] { basePosition }
                };

                placesCatalog.Add(place.id, place);
            }
#endregion

            placesCatalog = new Dictionary<string, IHotScenesController.PlaceInfo>();

            // Places in column shape
            // id: "{column}Column"
            AddPlaceInColumn(0, 4);
            AddPlaceInColumn(1, 4);
            AddPlaceInColumn(2, 4);
            AddPlaceInColumn(3, 4);

            // Places in square shape
            // id: "Square{basePosition}"
            AddPlaceInSquare(new Vector2Int(50, 50), 2);
            AddPlaceInSquare(new Vector2Int(-50, -50), 2);

            // Places with a single parcel
            // id: "{basePosition.x},{basePosition.y}"
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
