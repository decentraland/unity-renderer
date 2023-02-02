using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.ProfanityFiltering
{
    public class ProfanityWordProviderFromCsv : IProfanityWordProvider
    {
        private readonly string explicitCsvFilePath;
        private readonly string nonExplicitCsvFilePath;

        public ProfanityWordProviderFromCsv(string explicitCsvFilePath,
            string nonExplicitCsvFilePath)
        {
            this.explicitCsvFilePath = explicitCsvFilePath;
            this.nonExplicitCsvFilePath = nonExplicitCsvFilePath;
        }

        public IEnumerable<string> GetExplicitWords() => Load(explicitCsvFilePath);

        public IEnumerable<string> GetNonExplicitWords() => Load(nonExplicitCsvFilePath);

        private IEnumerable<string> Load(string csvFilePath)
        {
            var asset = Resources.Load<TextAsset>(csvFilePath);
            return asset.text.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
