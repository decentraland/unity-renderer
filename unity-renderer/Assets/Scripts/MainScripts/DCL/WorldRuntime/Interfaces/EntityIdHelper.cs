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
        private Regex regex;

        public EntityIdHelper()
        {
            regex = new Regex("^*[A-Z]",RegexOptions.Compiled);
        }
        
        public string GetOriginalId(long entityId)
        {
            if (!entityIdToLegacyId.ContainsKey(entityId))
                return SpecialEntityIdLegacyLiteral.SCENE_ROOT_ENTITY;

            return entityIdToLegacyId[entityId];
        }
        
        public long EntityFromLegacyEntityString(string entityId)
        {

            long entityIdLong = GetConvertedEntityId(entityId);
            return entityIdLong;
        }

        // public long GetConvertedEntityId(string entityId)
        // {
        //     long entityIdLong = 0;
        //    
        //     string entityIdExtracted = entityId.Substring(1);
        //     
        //     // It is not base 36, so we assign a negative number for it
        //     if (regex.IsMatch(entityIdExtracted))
        //     {
        //         if (negativeValues.ContainsKey(entityIdExtracted))
        //         {
        //             entityIdLong = negativeValues[entityIdExtracted];
        //         }
        //         else
        //         {
        //             entityIdLong = negativeCounter;
        //             negativeValues.Add(entityIdExtracted,negativeCounter);
        //             negativeCounter--;
        //         }
        //     }
        //     else
        //     {
        //         entityIdLong = DecodeBase36(entityIdExtracted) << 9;
        //     }
        //
        //     if (!entityIdToLegacyId.ContainsKey(entityIdLong))
        //         entityIdToLegacyId[entityIdLong] = entityId;
        //
        //     return entityIdLong;
        // }
        
        long GetConvertedEntityId(string entityId) {
            // entity Ids in base36 start with E
            if (entityId[0] == 'E')
            {
                long result = 0;
                long power = 1;
                for (var i = entityId.Length - 1; i > 0; i--)
                {
                    char charCode = entityId[i];
                    bool isBase36Number = charCode >= '0' /*'0'*/ && charCode <= '9'; /*'9'*/
                    bool isBase36Letter = charCode >= 'a' /*'a'*/ && charCode <= 'z'; /*'z'*/
                    if (isBase36Number)
                    {
                        result += (charCode - 48) /*'0'*/ * power;
                    } else if (isBase36Letter)
                    {
                        result += (charCode - 97) /*'a'*/ * power;
                    } else {
                        // non base36, fallback to entityIdFromDictionary
                        return entityIdFromDictionary(entityId);
                    }
                    power *= 36;
                    // this is slow result += charList.IndexOf(input[i]) * (long)Math.Pow(36, pos);
                }
                // reserve 512 entity ids (<<9)
                return result << 9;
            } else {
                // non standard entity, fallback to entityIdFromDictionary
                return entityIdFromDictionary(entityId);
            }
        }

        long entityIdFromDictionary(string entityId) {
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
            
            // It is not base 36, so we assign a negative number for it
            if (negativeValues.ContainsKey(entityId))
            {
                return negativeValues[entityId];
            } else
            {
                
                var newEntityIdLong = -negativeValues.Count -1;
                negativeValues.Add(entityId, newEntityIdLong);
                return newEntityIdLong;
            }
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