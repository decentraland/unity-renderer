using DCL.Controllers;
using System;
using UnityEngine;

namespace DCL.ContentModeration
{
    [Serializable]
    public record ContentModerationReportingModel
    {
        public SceneContentCategory currentRating;

        [Header("TEEN CONFIGURATION")]
        public string teenOptionTitle;
        public string teenOptionSubtitle;

        [Header("ADULT CONFIGURATION")]
        public string adultOptionTitle;
        public string adultOptionSubtitle;

        [Header("RESTRICTED CONFIGURATION")]
        public string restrictedOptionTitle;
        public string restrictedOptionSubtitle;
    }
}
