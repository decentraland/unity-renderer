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
                    negativeCounter++;
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
        
        public Int64 DecodeBase36(string input)
        {
            const string charList = "0123456789abcdefghijklmnopqrstuvwxyz";
            var reversed = input;
            long result = 0;
            int pos = 0;
            for(int i = reversed.Length-1;i >= 0; i--)
            {
                result += charList.IndexOf(reversed[i]) * (long)Math.Pow(36, pos);
                pos++;
            }
            return result;
        }
        
        public string ToBase36(long decimalNumber, int radix)
        {
            const int bitsInLong = 64;
            const string digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            if (radix < 2 || radix > digits.Length)
                throw new ArgumentException("The radix must be >= 2 and <= " + digits.Length);

            if (decimalNumber == 0)
                return "0";

            int index = bitsInLong - 1;
            long currentNumber = Math.Abs(decimalNumber);
            char[] charArray = new char[bitsInLong];

            while (currentNumber != 0)
            {
                int remainder = (int)(currentNumber % radix);
                charArray[index--] = digits[remainder];
                currentNumber = currentNumber / radix;
            }

            string result = new String(charArray, index + 1, bitsInLong - index - 1);
            if (decimalNumber < 0)
            {
                result = "-" + result;
            }

            return result;
        }
    }
}