using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.Controllers.LoadingScreenV2
{
    /// <summary>
    ///     RemoteHintRequestSource provides remote hint request functionality.
    ///     It retrieves hints asynchronously from a specified source URL using an ISourceWebRequestHandler.
    ///     The hints are parsed from the received JSON data using HintSceneParserUtil.
    /// </summary>
    public class RemoteHintRequestSource : IHintRequestSource
    {
        public RemoteHintRequestSource(string sourceUrlJson, SourceTag sourceTag, ISourceWebRequestHandler webRequestHandler)
        {
            Source = sourceUrlJson;
            this.SourceTag = sourceTag;
            LoadingHints = new List<Hint>();
            this.webRequestHandler = webRequestHandler;
        }

        public ISourceWebRequestHandler webRequestHandler { get; }
        public string Source { get; }
        public SourceTag SourceTag { get; }
        public List<Hint> LoadingHints { get; private set; }

        public async UniTask<List<Hint>> GetHintsAsync(CancellationToken ctx)
        {
            try
            {
                // If the CancellationToken is already canceled, return an empty list.
                if (ctx.IsCancellationRequested) { return LoadingHints; }

                string json = await webRequestHandler.Get(Source);

                if (!string.IsNullOrEmpty(json)) { LoadingHints = HintSceneParserUtil.ParseJsonToHints(json); }
            }
            catch (Exception ex) { Debug.LogWarning($"Exception in RemoteHintRequestSource.GetHintsAsync: {ex.Message}\n{ex.StackTrace}"); }

            return LoadingHints;
        }

        public void Dispose()
        {
            LoadingHints.Clear();
        }
    }
}
