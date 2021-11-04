using System;
using System.Collections.Generic;
using UnityEngine;

public class ProfanityWordProviderFromCsv : IProfanityWordProvider
{
    private readonly string csvFilePath;

    public ProfanityWordProviderFromCsv(string csvFilePath)
    {
        this.csvFilePath = csvFilePath;
    }

    public IEnumerable<string> GetAll()
    {
        var asset = Resources.Load<TextAsset>(csvFilePath);
        return asset.text.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
    }
}