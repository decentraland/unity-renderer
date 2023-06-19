using Cysharp.Threading.Tasks;
using DCL.Emotes;
using System;
using UnityEngine;

namespace DCLServices.EmotesCatalog
{
    [Serializable]
    public class EmoteEntityDto
    {
        [Serializable]
        public class ContentDto
        {
            public string file;
            public string hash;
        }

        [Serializable]
        public class MetadataDto
        {
            [Serializable]
            public class Representation
            {
                public string[] bodyShapes;
                public string mainFile;
                public string[] contents;
            }

            [Serializable]
            public class DataDto
            {
                public Representation[] representations;
                public string category;
                public string[] tags;
                public bool loop;
            }

            public DataDto emoteDataADR74;
            public string id;

            public i18n[] i18n;
            public string thumbnail;

            public string rarity;
            public string description;
        }

        public MetadataDto metadata;
        public ContentDto[] content;
        public string id;

        public string GetContentHashByFileName(string fileName)
        {
            foreach (ContentDto dto in content)
                if (dto.file == fileName)
                    return dto.hash;

            return null;
        }

        public WearableItem ToWearableItem(string contentBaseUrl)
        {
            WearableItem wearable = new WearableItem();
            wearable.entityId = id;
            wearable.emoteDataV0 = null;

            MetadataDto metadataDto = metadata;
            if (metadataDto == null) return wearable;

            wearable.description = metadataDto.description;
            wearable.rarity = metadataDto.rarity;
            wearable.i18n = metadataDto.i18n;
            wearable.id = metadataDto.id;
            wearable.thumbnail = GetContentHashByFileName(metadataDto.thumbnail);

            var data = metadataDto.emoteDataADR74;
            if (data == null) return wearable;

            // todo: remove this when we refactor the WearableItem into EmoteItem
            wearable.emoteDataV0 = new EmoteDataV0() { loop = data.loop };

            wearable.data = new WearableItem.Data
            {
                representations = new WearableItem.Representation[data.representations.Length],
                category = data.category,
                tags = data.tags,
                loop = data.loop
            };

            for (var i = 0; i < data.representations.Length; i++)
            {
                MetadataDto.Representation representation = data.representations[i];

                wearable.data.representations[i] = new WearableItem.Representation
                {
                    bodyShapes = representation.bodyShapes,
                    mainFile = representation.mainFile,
                    contents = new WearableItem.MappingPair[representation.contents.Length],
                };

                for (var z = 0; z < representation.contents.Length; z++)
                {
                    string fileName = representation.contents[z];
                    string hash = GetContentHashByFileName(fileName);

                    wearable.data.representations[i].contents[z] = new WearableItem.MappingPair
                    {
                        url = $"{contentBaseUrl}/{hash}",
                        hash = hash,
                        key = fileName,
                    };
                }
            }

            return wearable;
        }
    }
}
