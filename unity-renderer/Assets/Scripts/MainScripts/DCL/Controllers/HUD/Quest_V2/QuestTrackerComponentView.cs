using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Quest
{
    public class QuestTrackerComponentView : BaseComponentView, IQuestTrackerComponentView
    {
        [SerializeField] private Transform stepsContainer;

        public override void RefreshControl()
        {
        }

        
    }

}
