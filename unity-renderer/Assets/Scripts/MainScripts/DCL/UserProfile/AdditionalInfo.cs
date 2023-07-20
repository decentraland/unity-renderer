using System;

namespace DCL.UserProfiles
{
    public record AdditionalInfo
    {
        public string Country { get; set; }
        public string EmploymentStatus { get; set; }
        public string Gender { get; set; }
        public string Pronouns { get; set; }
        public string RelationshipStatus { get; set; }
        public string SexualOrientation { get; set; }
        public string Language { get; set; }
        public string Profession { get; set; }
        public DateTime? BirthDate { get; set; }
        public string RealName { get; set; }
        public string Hobbies { get; set; }

        public AdditionalInfo()
        {
        }

        public AdditionalInfo(AdditionalInfo other)
        {
            CopyFrom(other);
        }

        public void CopyFrom(AdditionalInfo additionalInfo)
        {
            Country = additionalInfo.Country;
            EmploymentStatus = additionalInfo.EmploymentStatus;
            Gender = additionalInfo.Gender;
            Pronouns = additionalInfo.Pronouns;
            RelationshipStatus = additionalInfo.RelationshipStatus;
            SexualOrientation = additionalInfo.SexualOrientation;
            Language = additionalInfo.Language;
            Profession = additionalInfo.Profession;
            BirthDate = additionalInfo.BirthDate;
            RealName = additionalInfo.RealName;
            Hobbies = additionalInfo.Hobbies;
        }

        public virtual bool Equals(AdditionalInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Country == other.Country && EmploymentStatus == other.EmploymentStatus
                                            && Gender == other.Gender
                                            && Pronouns == other.Pronouns
                                            && RelationshipStatus == other.RelationshipStatus
                                            && SexualOrientation == other.SexualOrientation
                                            && Language == other.Language
                                            && Profession == other.Profession
                                            && Nullable.Equals(BirthDate, other.BirthDate)
                                            && RealName == other.RealName
                                            && Hobbies == other.Hobbies;
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Country);
            hashCode.Add(EmploymentStatus);
            hashCode.Add(Gender);
            hashCode.Add(Pronouns);
            hashCode.Add(RelationshipStatus);
            hashCode.Add(SexualOrientation);
            hashCode.Add(Language);
            hashCode.Add(Profession);
            hashCode.Add(BirthDate);
            hashCode.Add(RealName);
            hashCode.Add(Hobbies);
            return hashCode.ToHashCode();
        }
    }
}
