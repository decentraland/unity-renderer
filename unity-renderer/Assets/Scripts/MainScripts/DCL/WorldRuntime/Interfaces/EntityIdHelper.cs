using System.Collections.Generic;
using DCL.Models;

namespace DCL
{
    public class EntityIdHelper
    {
        private Dictionary<long, string> entityIdToLegacyId = new Dictionary<long, string>();

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

            long entityIdLong = entityId.GetHashCode() << 9;

            if (!entityIdToLegacyId.ContainsKey(entityIdLong))
                entityIdToLegacyId[entityIdLong] = entityId;

            return entityIdLong;
        }
    }
}