using System;

namespace KernelConfigurationTypes
{
    [Serializable]
    public class Features
    {
        public bool enableBuilderInWorld = false;
        public bool enableTutorial = true;
        public bool enablePeopleCounter = false;
        public bool enableExploreV2 = false;

        public override bool Equals(object obj) { return obj is Features other && Equals(other); }
        protected bool Equals(Features other)
        {
            return enableBuilderInWorld == other.enableBuilderInWorld
                   && enableTutorial == other.enableTutorial
                   && enablePeopleCounter == other.enablePeopleCounter
                   && enableExploreV2 == other.enableExploreV2;
        }
        public Features Clone()
        {
            Features clone = (Features)this.MemberwiseClone();
            return clone;
        }
    }

    [Serializable]
    public class Comms
    {
        public float commRadius = 4;
        public bool voiceChatEnabled = false;

        public bool Equals(Comms other)
        {
            return commRadius == other?.commRadius &&
                   voiceChatEnabled == other?.voiceChatEnabled;
        }

        public Comms Clone()
        {
            Comms clone = (Comms)this.MemberwiseClone();
            return clone;
        }
    }

    [Serializable]
    public class Profiles
    {
        public string nameValidCharacterRegex = "";
        public string nameValidRegex = "";

        public bool Equals(Profiles other)
        {
            return nameValidCharacterRegex == other?.nameValidCharacterRegex &&
                   nameValidRegex == other?.nameValidRegex;
        }

        public Profiles Clone()
        {
            Profiles clone = (Profiles)this.MemberwiseClone();
            return clone;
        }
    }

    [Serializable]
    public class WorldRange
    {
        public int xMin = 0;
        public int xMax = 0;
        public int yMin = 0;
        public int yMax = 0;

        public bool Equals(WorldRange other)
        {
            return xMin == other?.xMin &&
                   xMax == other?.xMax &&
                   yMin == other?.yMin &&
                   yMax == other?.yMax;
        }

        public WorldRange Clone()
        {
            WorldRange clone = (WorldRange)this.MemberwiseClone();
            return clone;
        }
        public WorldRange(int xMin, int yMin, int xMax, int yMax)
        {
            this.xMin = xMin;
            this.yMin = yMin;
            this.xMax = xMax;
            this.yMax = yMax;
        }

        public bool Contains(int x, int y)
        {
            return x >= xMin && x <= xMax &&
                   y >= yMin && y <= yMax;
        }
    }

    [Serializable]
    public class Debugging
    {
        public bool sceneDebugPanelEnabled = false;
        public int sceneDebugPanelTargetSceneNumber = -1;
        public int sceneLimitsWarningSceneNumber = -1;

        public bool Equals(Debugging other)
        {
            return sceneDebugPanelEnabled == other?.sceneDebugPanelEnabled &&
                   sceneDebugPanelTargetSceneNumber == other?.sceneDebugPanelTargetSceneNumber &&
                   sceneLimitsWarningSceneNumber == other?.sceneLimitsWarningSceneNumber;
        }

        public Debugging Clone()
        {
            Debugging clone = (Debugging)this.MemberwiseClone();
            return clone;
        }
    }

    [Serializable]
    public class ProceduralSkybox
    {
        public string configToLoad = "Generic_Skybox";
        public float lifecycleDuration = 120;
        public float fixedTime = -1;
        public bool disableReflection = false;
        public float updateReflectionTime = -1;     // in mins

        public bool Equals(ProceduralSkybox other)
        {
            return configToLoad == other?.configToLoad &&
                   lifecycleDuration == other?.lifecycleDuration &&
                   fixedTime == other?.fixedTime;
        }

        public ProceduralSkybox Clone()
        {
            ProceduralSkybox clone = (ProceduralSkybox)this.MemberwiseClone();
            return clone;
        }
    }
}