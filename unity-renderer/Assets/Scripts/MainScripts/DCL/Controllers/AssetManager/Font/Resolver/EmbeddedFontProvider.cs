using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class EmbeddedFontProvider : IFontAssetProvider
{
    private const string RESOURCE_FONT_FOLDER = "Fonts & Materials";
    private const string DEFAULT_SANS_SERIF_HEAVY = "Inter-Heavy SDF";
    private const string DEFAULT_SANS_SERIF_BOLD = "Inter-Bold SDF";
    private const string DEFAULT_SANS_SERIF_SEMIBOLD = "Inter-SemiBold SDF";
    private const string DEFAULT_SANS_SERIF = "Inter-Regular SDF";

    private readonly Dictionary<string, string> fontsMapping = new Dictionary<string, string>()
    {
        { "builtin:SF-UI-Text-Regular SDF", DEFAULT_SANS_SERIF },
        { "builtin:SF-UI-Text-Heavy SDF", DEFAULT_SANS_SERIF_HEAVY },
        { "builtin:SF-UI-Text-Semibold SDF", DEFAULT_SANS_SERIF_SEMIBOLD },
        { "builtin:LiberationSans SDF", "LiberationSans SDF" },
        { "SansSerif", DEFAULT_SANS_SERIF },
        { "SansSerif_Heavy", DEFAULT_SANS_SERIF_HEAVY },
        { "SansSerif_Bold", DEFAULT_SANS_SERIF_BOLD },
        { "SansSerif_SemiBold", DEFAULT_SANS_SERIF_SEMIBOLD },
    };


    public async UniTask<TMP_FontAsset> GetFontAsync(string src, CancellationToken cancellationToken = default)
    {
        if (!fontsMapping.TryGetValue(src, out string fontResourceName))
            throw new Exception("Font doesn't correspond with any know font");

        ResourceRequest request = Resources.LoadAsync($"{RESOURCE_FONT_FOLDER}/{fontResourceName}", typeof(TMP_FontAsset));

        await request.WithCancellation(cancellationToken);

        if (request.asset != null)
            return request.asset as TMP_FontAsset;
        else
            return null;
    }

}
