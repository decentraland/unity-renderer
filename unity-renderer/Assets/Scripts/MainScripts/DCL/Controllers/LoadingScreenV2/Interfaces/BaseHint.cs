using DCL.Controllers.LoadingScreenV2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Controllers.LoadingScreenV2
{
    [System.Serializable]
    public class BaseHint : IHint
    {
        [SerializeField] private string textureUrl;
        [SerializeField] private string title;
        [SerializeField] private string body;
        [SerializeField] private SourceTag sourceTag;

        public string TextureUrl => textureUrl;
        public string Title => title;
        public string Body => body;
        public SourceTag SourceTag => sourceTag;

        public BaseHint(string textureUrl, string title, string body, SourceTag sourceTag)
        {
            this.textureUrl = textureUrl;
            this.title = title;
            this.body = body;
            this.sourceTag = sourceTag;
        }

        public void Dispose()
        {
        }
    }
}

