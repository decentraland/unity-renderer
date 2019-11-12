using System.Collections;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

//Since this responsibility will be moved to Explorer take this retriever as a temporary workaround
public class MinimapMetadataRetriever : MonoBehaviour
{
    private const string DCL_TILES_API = "https://api.decentraland.org/v1/tiles";

    public void Awake()
    {
        MinimapMetadata.GetMetadata().UpdateData(null);
        GetData();
    }

    public void GetData()
    {
        StartCoroutine(MetadataWebRequest(DCL_TILES_API));
    }

    IEnumerator MetadataWebRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else
            {
                StartCoroutine(ParseMinimapMetadata(webRequest.downloadHandler.text));
            }
        }
    }
    private static IEnumerator ParseMinimapMetadata(string text)
    {
        JObject jsonResponse = JObject.Parse(text);
        MinimapMetadata.Model model = new MinimapMetadata.Model(Vector2Int.one * -150, Vector2Int.one * 150);

        for (int i = model.bottomLeftCorner.x; i <= model.topRightCorner.x; i++)
        {
            for (int j = model.bottomLeftCorner.y; j <= model.topRightCorner.y; j++)
            {
                var jsonTile = jsonResponse["data"][i + "," + j];
                var position = new Vector2Int(jsonTile["x"].Value<int>(), jsonTile["y"].Value<int>());
                var tileType = jsonTile["type"].Value<int>();
                var tileName = "";
                if (jsonTile["name"] != null)
                {
                    tileName = jsonTile["name"].Value<string>();
                }

                model.AddTile(i, j, new MinimapMetadata.Tile(position, tileType, tileName));
            }
            yield return null;
        }

        MinimapMetadata.GetMetadata().UpdateData(model);
    }
}