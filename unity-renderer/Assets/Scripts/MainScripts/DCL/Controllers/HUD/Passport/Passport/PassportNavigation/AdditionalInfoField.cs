using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DCL.Social.Passports
{
    [Serializable]
    public enum AdditionalInfoField
    {
        GENDER,
        COUNTRY,
        BIRTH_DATE,
        PRONOUNS,
        RELATIONSHIP_STATUS,
        SEXUAL_ORIENTATION,
        LANGUAGE,
        PROFESSION,
        HOBBIES,
        REAL_NAME,
        EMPLOYMENT_STATUS
    }

    public static class AdditionalInfoFieldToStringExtensions
    {
        public static string ToName(this AdditionalInfoField field)
        {
            switch (field)
            {
                case AdditionalInfoField.BIRTH_DATE:
                    return "BIRTH DATE";
                case AdditionalInfoField.RELATIONSHIP_STATUS:
                    return "RELATIONSHIP STATUS";
                case AdditionalInfoField.SEXUAL_ORIENTATION:
                    return "SEXUAL ORIENTATION";
                case AdditionalInfoField.REAL_NAME:
                    return "REAL NAME";
                case AdditionalInfoField.EMPLOYMENT_STATUS:
                    return "EMPLOYMENT STATUS";
                default:
                    return field.ToString();
            }
        }
    }
}
