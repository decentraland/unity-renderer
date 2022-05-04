using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DCL.Models;
using UnityEngine;

namespace DCL
{
    public class EntityIdHelper
    {
        internal Dictionary<long, string> entityIdToLegacyId = new Dictionary<long, string>();
        internal Dictionary<string, long> negativeValues = new Dictionary<string, long>();

        private int negativeCounter = -1;
        
        public string GetOriginalId(long entityId)
        {
            if (!entityIdToLegacyId.ContainsKey(entityId))
                return SpecialEntityIdLegacyLiteral.SCENE_ROOT_ENTITY;

            return entityIdToLegacyId[entityId];
        }

        public long EntityFromLegacyEntityString(string entityId)
        {
            switch (entityId)
            {
                case SpecialEntityIdLegacyLiteral.SCENE_ROOT_ENTITY:
                    return (long) SpecialEntityId.SCENE_ROOT_ENTITY;

                case SpecialEntityIdLegacyLiteral.FIRST_PERSON_CAMERA_ENTITY_REFERENCE:
                    return (long) SpecialEntityId.FIRST_PERSON_CAMERA_ENTITY_REFERENCE;

                case SpecialEntityIdLegacyLiteral.AVATAR_ENTITY_REFERENCE:
                    return (long) SpecialEntityId.AVATAR_ENTITY_REFERENCE;

                case SpecialEntityIdLegacyLiteral.AVATAR_POSITION_REFERENCE:
                    return (long) SpecialEntityId.AVATAR_POSITION_REFERENCE;

                case SpecialEntityIdLegacyLiteral.THIRD_PERSON_CAMERA_ENTITY_REFERENCE:
                    return (long) SpecialEntityId.THIRD_PERSON_CAMERA_ENTITY_REFERENCE;
            }
            
            long entityIdLong = GetConvertedEntityId(entityId);
            return entityIdLong;
        }

        public long GetConvertedEntityId(string entityId)
        {
            long entityIdLong = 0;
            var regex = new Regex("^*[A-Z]");
            entityId = entityId.Substring(1);
            
            // It is not base 36, so we assign a negative number for it
            if (regex.IsMatch(entityId))
            {
                if (negativeValues.ContainsKey(entityId))
                {
                    entityIdLong = negativeValues[entityId];
                }
                else
                {
                    entityIdLong = negativeCounter;
                    negativeValues.Add(entityId,negativeCounter);
                    negativeCounter--;
                }
            }
            else
            {
                entityIdLong = DecodeBase36(entityId) << 9;
            }

            if (!entityIdToLegacyId.ContainsKey(entityIdLong))
                entityIdToLegacyId[entityIdLong] = entityId;

            return entityIdLong;
        }
        
        public long DecodeBase36(string input)
        {
            const string charList = "0123456789abcdefghijklmnopqrstuvwxyz";
            long result = 0;
            int pos = 0;
            for(int i = input.Length-1;i >= 0; i--)
            {
                result += charList.IndexOf(input[i]) * (long)Math.Pow(36, pos);
                pos++;
            }
            return result;
        }
    }
}