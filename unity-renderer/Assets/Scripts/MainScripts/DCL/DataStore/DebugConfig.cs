﻿using UnityEngine;
using UnityEngine.Serialization;
using Variables.SpawnPoints;

namespace DCL
{
    [System.Serializable]
    public class DebugConfig
    {
        /// <summary>
        /// If enabled, filters all scene messages that are not soloSceneCoords
        /// </summary>
        public bool soloScene;

        /// <summary>
        /// Used in conjunction with soloScene
        /// </summary>
        public Vector2Int soloSceneCoords;

        /// <summary>
        /// Enabled when SetDebug() is sent by kernel. Usually coupled with the DEBUG_MODE url param.
        /// </summary>
        [System.NonSerialized]
        public BaseVariable<bool> isDebugMode = new BaseVariable<bool>();
        
        [System.NonSerialized]
        public BaseVariable<bool> isFPSPanelVisible = new BaseVariable<bool>();
        
        [System.NonSerialized]
        public BaseDictionary<string, bool> showSceneBoundingBoxes = new BaseDictionary<string, bool>();
        
        [System.NonSerialized]
        public  BaseVariable<bool> isPreviewMenuActive = new BaseVariable<bool>();
        
        [System.NonSerialized]
        public BaseDictionary<string, SceneSpawnPointsData> showSceneSpawnPoints = new BaseDictionary<string, SceneSpawnPointsData>();

        [System.NonSerialized]
        public BaseVariable<bool> showSceneABDetectionLayer = new BaseVariable<bool>();
        
        [System.NonSerialized]
        public BaseVariable<bool> showGlobalABDetectionLayer = new BaseVariable<bool>();
        
        [System.NonSerialized]
        public BaseVariable<float> jsHeapSizeLimit = new BaseVariable<float>();
        
        [System.NonSerialized]
        public BaseVariable<float> jsTotalHeapSize = new BaseVariable<float>();
        
        [System.NonSerialized]
        public BaseVariable<float> jsUsedHeapSize = new BaseVariable<float>();

        /// <summary>
        /// True when WSS message pipeline is enabled
        /// </summary>
        [System.NonSerialized]
        public bool isWssDebugMode;

        /// <summary>
        /// Do not send global scenes messages (i.e. avatars)
        /// </summary>
        public bool ignoreGlobalScenes = false;

        /// <summary>
        /// do Debug.Break() and log when processing each message
        /// </summary>
        public bool msgStepByStep = false;
    }
}