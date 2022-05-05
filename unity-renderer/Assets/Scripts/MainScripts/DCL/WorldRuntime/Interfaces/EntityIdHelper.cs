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
            if (entityId < 512) {
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
            
            if ((entityId & 0x80000000) != 0 && entityIdToLegacyId.ContainsKey(entityId)) {
                // the entity has the 0x80000000 mask signaling it was an invalid ID
                return entityIdToLegacyId[entityId];
            }

            return ToBase36EntityId(entityId);
        }
        
        private static string ToBase36EntityId(long value)
        {
            const string base36 = "0123456789abcdefghijklmnopqrstuvwxyz";
            // this can be static in the class
            var sb = new StringBuilder(13);

            value = value >> 9;
            value -= 1;
            do
            {
                sb.Insert(0, base36[(byte)(value % 36)]);
                value /= 36;
            } while (value != 0);
            sb.Insert(0, "E"); // E prefix for entities
			return sb.ToString();
        }
        
        public long EntityFromLegacyEntityString(string entityId)
        {
            long entityIdLong = GetConvertedEntityId(entityId);
            return entityIdLong;
        }

        long GetConvertedEntityId(string entityId) {
            // entity Ids in base36 start with E
            if (entityId[0] == 'E')
            {
                long result = 1;
                long power = 1;
                for (var i = entityId.Length - 1; i > 0; i--)
                {
                    char charCode = entityId[i];
                    bool isBase36Number = charCode >= '0' && charCode <= '9';
                    bool isBase36Letter = charCode >= 'a' && charCode <= 'z';
                    if (isBase36Number)
                    {
                        result += (charCode - '0') * power;
                    } else if (isBase36Letter)
                    {
                        result += (10 + (charCode - 'a')) * power;
                    } else {
                        // non base36, fallback to entityIdFromDictionary
                        return entityIdFromDictionary(entityId);
                    }
                    power *= 36;
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
            if (invalidEntities.ContainsKey(entityId)) {
                return invalidEntities[entityId];
            } else {
                var newEntityIdLong = ++invalidEntityCounter | 0x80000000;
				
                if (!entityIdToLegacyId.ContainsKey(newEntityIdLong))
                    entityIdToLegacyId[newEntityIdLong] = entityId;
					
                invalidEntities.Add(entityId, newEntityIdLong);
                return newEntityIdLong;
            }
        }
    }
    /*
    // test cases
    
	public void test(string s, long expected) {
		Console.WriteLine("For string {0}", s);
		var	r = GetConvertedEntityId(s);
		if (r != expected) 
			Console.WriteLine("  ERROR! Num expected={0} (0x{0:X}) given={1} (0x{1:X})", expected, r);
		else
			Console.WriteLine("  OK     Num expected={0} (0x{0:X}) given={1} (0x{1:X})", expected, r);

		var gen = GetOriginalId(r);
		if (gen != s) 
			Console.WriteLine("  ERROR! Gen expected={0} given={1}", s, gen);
		else
			Console.WriteLine("  OK     Gen expected={0} given={1}", s, gen);
	}
    
    t.test("0", 0);
		t.test("AvatarEntityReference", 1);
		t.test("AvatarPositionEntityReference", 2);
		t.test("FirstPersonCameraEntityReference", 3);
		t.test("PlayerEntityReference", 4);
		t.test("1", 0x80000000);
		t.test("2", 0x80000001);
		t.test("1", 0x80000000);
		t.test("E0", 1 << 9);		
		t.test("E1", 2 << 9);
		t.test("Eb", 12 << 9);
		t.test("E33", 112 << 9);
		t.test("Euv", 1112 << 9);
		t.test("E8kn", 11112 << 9);
		t.test("E2dqf",111112 << 9);
		t.test("Ea", 11 << 9);
		t.test("Eaa", 371 << 9);
		t.test("Eaaa", 13331 << 9);
		//repeated:
        t.test("E1", 2 << 9);
		t.test("Eijealm", 0x85A1505600);
    
    */
}
