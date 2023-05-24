using Cysharp.Threading.Tasks;
using DCL.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL.Controllers.LoadingScreenV2
{
    ///<summary>
    /// RemoteHintRequestSource provides remote hint request functionality.
    /// It retrieves hints asynchronously from a specified source URL using an ISourceWebRequestHandler.
    /// The hints are parsed from the received JSON data using HintSceneParserUtil.
    ///</summary>
    public class RemoteHintRequestSource : IHintRequestSource
    {
        public string source { get; }
        public SourceTag sourceTag { get; }
        public List<Hint> loading_hints { get; private set; }
        public ISourceWebRequestHandler webRequestHandler { get; }

        public RemoteHintRequestSource(string sourceUrlJson, SourceTag sourceTag, ISourceWebRequestHandler webRequestHandler)
        {
            this.source = sourceUrlJson;
            this.sourceTag = sourceTag;
            this.loading_hints = new List<Hint>();
            this.webRequestHandler = webRequestHandler;
        }

        public async UniTask<List<Hint>> GetHintsAsync(CancellationToken ctx)
        {
            try
            {
                // If the CancellationToken is already canceled, return an empty list.
                if (ctx.IsCancellationRequested)
                {
                    return loading_hints;
                }

                string json = await webRequestHandler.Get(source);

                if (!string.IsNullOrEmpty(json))
                {
                    loading_hints = HintSceneParserUtil.ParseJsonToHints(json);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Exception in RemoteHintRequestSource.GetHintsAsync: {ex.Message}\n{ex.StackTrace}");
            }

            return loading_hints;
        }

        public void Dispose()
        {
            loading_hints.Clear();
        }
    }

}
