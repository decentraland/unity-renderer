using Cysharp.Threading.Tasks;
using DCL.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL.Controllers.LoadingScreenV2
{
    public class RemoteHintRequestSource : IHintRequestSource
    {
        public string source { get; }
        public SourceTag sourceTag { get; }
        public List<IHint> loading_hints { get; private set; }
        public ISourceWebRequestHandler webRequestHandler { get; }

        public RemoteHintRequestSource(string sourceUrlJson, SourceTag sourceTag, ISourceWebRequestHandler webRequestHandler)
        {
            this.source = sourceUrlJson;
            this.sourceTag = sourceTag;
            this.loading_hints = new List<IHint>();
            this.webRequestHandler = webRequestHandler;
        }

        public async UniTask<List<IHint>> GetHintsAsync(CancellationToken ctx)
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
                    loading_hints = HintSceneParser.ParseJsonToHints(json);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception in RemoteHintRequestSource.GetHintsAsync: {ex.Message}\n{ex.StackTrace}");
            }

            return loading_hints;
        }

        public void Dispose()
        {
            loading_hints.Clear();
        }
    }

}
