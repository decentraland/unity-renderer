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
        internal Dictionary<string, long> invalidEntities = new Dictionary<string, long>();

        private int invalidEntityCounter = -1;
        
        public string GetOriginalId(long entityId)
        {
            // if the entityId is less than 512, then it is considered well-known.
            if (entityId < 512) 
            {
                switch (entityId)
                {
                    case (long)SpecialEntityId.SCENE_ROOT_ENTITY:
                        return SpecialEntityIdLegacyLiteral.SCENE_ROOT_ENTITY;

                    case (long)SpecialEntityId.FIRST_PERSON_CAMERA_ENTITY_REFERENCE:
                        return SpecialEntityIdLegacyLiteral.FIRST_PERSON_CAMERA_ENTITY_REFERENCE;

                    case (long)SpecialEntityId.AVATAR_ENTITY_REFERENCE:
                        return SpecialEntityIdLegacyLiteral.AVATAR_ENTITY_REFERENCE;

                    case (long)SpecialEntityId.AVATAR_POSITION_REFERENCE:
                        return SpecialEntityIdLegacyLiteral.AVATAR_POSITION_REFERENCE;

                    case (long)SpecialEntityId.THIRD_PERSON_CAMERA_ENTITY_REFERENCE:
                        return SpecialEntityIdLegacyLiteral.THIRD_PERSON_CAMERA_ENTITY_REFERENCE;
                }
            }
            
            // if it has the 0x8000_0000 mask, then it *should* be stored in the entityIdToLegacyId map
            if ((entityId & 0x80000000) != 0 && entityIdToLegacyId.ContainsKey(entityId)) 
            {
                // the entity has the 0x80000000 mask signaling it was an invalid ID
                return entityIdToLegacyId[entityId];
            }

            // lastly, fallback to generating the entityId based on the number.
            return ToBase36EntityId(entityId);
        }
        
        private string ToBase36EntityId(long value)
        {
            // TODO(mendez): the string builder approach can be better. It does use more allocations
            //               than the optimal. The ideal scenario would use a `stackalloc char[13]` instead 
            //               of StringBuilder but my knowledge and tools for C# are limited at the moment
            const string base36 = "0123456789abcdefghijklmnopqrstuvwxyz";
            var sb = new StringBuilder(13);

            value = value >> 9; // recover from bit-shift
            value -= 1; // recover the added 1

            do
            {
                sb.Insert(0, base36[(byte)(value % 36)]);
                value /= 36;
            } while (value != 0);

            // E prefix for entities
            sb.Insert(0, "E");

            return sb.ToString();
        }
        
        public long EntityFromLegacyEntityString(string entityId)
        {
            long entityIdLong = GetConvertedEntityId(entityId);
            return entityIdLong;
        }

        private long GetConvertedEntityId(string entityId)
        {
            // entity Ids in base36 start with E
            if (entityId[0] == 'E')
            {
                long result = 1;
                long power = 1;
                for (var i = entityId.Length - 1; i > 0; i--)
                {
                    char charCode = entityId[i];
                    bool isNumber = charCode >= '0' && charCode <= '9';
                    bool isBase36Letter = charCode >= 'a' && charCode <= 'z';
                    if (isNumber)
                        result += (charCode - '0') * power;
                    else if (isBase36Letter)
                        result += (10 + (charCode - 'a')) * power;
                    else // non base36, fallback to entityIdFromDictionary
                        return entityIdFromDictionary(entityId);

                    power *= 36;
                }
                
                // reserve 512 entity ids (<<9)
                long newEntityIdLong = result << 9;

                return newEntityIdLong;
            }
            
            // non standard entity, fallback to entityIdFromDictionary
            return entityIdFromDictionary(entityId);
        }

        private long entityIdFromDictionary(string entityId)
        {
            // first we try against well known and lesser used entities
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

            // secondly, test if it was already seen in invalidEntities
            if (invalidEntities.ContainsKey(entityId)) 
            {
                return invalidEntities[entityId];
            }
            else 
            {
                // generate the new ID, uses the mask 0x8000_0000
                var newEntityIdLong = ++invalidEntityCounter | 0x80000000;
				
                // store the mapping from newEntityIdLong->original
                if (!entityIdToLegacyId.ContainsKey(newEntityIdLong))
                    entityIdToLegacyId[newEntityIdLong] = entityId;
					
                // store the mapping original->newEntityIdLong
                invalidEntities.Add(entityId, newEntityIdLong);

                return newEntityIdLong;
            }
        }
    }
}
