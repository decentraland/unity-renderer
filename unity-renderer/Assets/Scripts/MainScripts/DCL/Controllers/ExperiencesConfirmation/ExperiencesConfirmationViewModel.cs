using System.Collections.Generic;

namespace DCL.PortableExperiences.Confirmation
{
    public record ExperiencesConfirmationViewModel
    {
        public string Name;
        public List<string> Permissions;
        public string IconUrl;
        public string Description;
        public bool IsSmartWearable;
    }
}
