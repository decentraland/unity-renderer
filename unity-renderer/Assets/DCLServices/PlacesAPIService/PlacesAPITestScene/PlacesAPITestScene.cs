using DCL;
using Newtonsoft.Json;
using UnityEngine;

namespace DCLServices.PlacesAPIService.PlacesAPITestScene
{
    /// <summary>
    /// Lot of calls to the client need to be signed, therefore (until we have a proper way to sign them in Unity) we depend on kernel.
    /// That's the reason this cannot be in an isolated test scene, just log in normally into the world and add the script
    /// </summary>
    public class PlacesAPITestScene : MonoBehaviour
    {
        public string parcelId;
        public Vector2Int coords;

        private PlacesAPIService service;
        private PlacesAPIClient client;
        [ContextMenu("Initialize")]
        public void Initialize()
        {
            client = new PlacesAPIClient(Environment.i.serviceLocator.Get<IWebRequestController>());
            service = new PlacesAPIService(client);
        }

        [ContextMenu("Client_GetMostActivePlaces")]
        public async void Client_GetMostActivePlaces()
        {
            var result =await client.GetMostActivePlaces(1, 10, default);
            Debug.Log(JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        [ContextMenu("Client_GetPlaceId")]
        public async void Client_GetPlaceId()
        {
            var result = await client.GetPlace(parcelId, default);
            Debug.Log(JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        [ContextMenu("Client_GetPlaceCoords")]
        public async void Client_GetPlaceCoords()
        {
            var result = await client.GetPlace(coords, default);
            Debug.Log(JsonConvert.SerializeObject(result, Formatting.Indented));

        }
        [ContextMenu("Client_GetFavorites(")]
        public async void Client_GetFavorites()
        {
            var result =await client.GetFavorites(default);
            Debug.Log(result.Count);
            Debug.Log(JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        [ContextMenu("Client_SetFavorites(")]
        public async void Client_SetFavorites()
        {
            await client.SetPlaceFavorite(parcelId, true,default);
            Debug.Log("Added as fav");
        }

    }
}
