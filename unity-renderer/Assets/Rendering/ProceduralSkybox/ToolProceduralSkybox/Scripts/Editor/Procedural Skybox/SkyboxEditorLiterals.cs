using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    public static class SkyboxEditorLiterals
    {
        // Paths
        public static class Paths
        {
            public const string toolMeasurementPath = "Assets/Rendering/ProceduralSkybox/ToolProceduralSkybox/Scripts/Editor/EditorToolSize.asset";
        }

        // Characters and signs
        public static class Characters
        {
            public const char upArrow = '\u25B2';
            public const char downArrow = '\u25BC';
            public const char renderMarker = '\u29BF';
            public const string sign_add = "+";
            public const string sign_goto = ">";
            public const string sign_remove = "-";
        }

        // Layers
        public static class Layers
        {
            public const string layerName = "Layer Name";
            public const string layerType = "Layer Type";
            public const string backgroundLayer = "Background Layer";
            public const string ambientLayer = "Ambient Layer";
            public const string avatarLayer = "Avatar Layer";
            public const string fogLayer = "Fog Layer";
            public const string directionalLightLayer = "Directional Light Layer";
            public const string twoDLayers = "2D Layers";
            public const string RenderDomeLayers = "Dome Layers";
            public const string RenderSatelliteLayers = "Satellite Layers";
            public const string RenderPlanarLayers = "Planar Layers";
            public const string domeName = "Dome Name";
            public const string domeSize = "Dome Size";
            public const string horizonPlane = "Horizon Plane";
        }

        // Labels and ButtonNames
        public static class Labels
        {
            public const string timeLineTags = "Timeline Tags";
            public const string config = "Config";
            public const string name = "Name";
            public const string create = "Create";
            public const string cancel = "Cancel";
            public const string currentProfile = "Current Profile";
            public const string play = "Play";
            public const string pause = "Pause";
            public const string cycle = "cycle(minutes)";
            public const string pin = "Pin";
            public const string unpin = "Unpin";
            public const string sunObjectName = "The Sun_Temp";
            public const string defaultskyboxName = "Generic Skybox";
            public const string useDirecnLight = "Use Directional Light";
            public const string inWorld = "In World";
            public const string colorGradient = "Color Gradient";
            public const string realtimeLightColor = "Realtime Light Color";
            public const string inEditorBackpack = "In Editor (Backpack)";
            public const string useGradient = "Use Gradient";
            public const string useFog = "Use Fog";
        }

        // Layer Properties
        public static class LayerProperties
        {
            public const string cubemap = "Cubemap";
            public const string rowsColumns = "Rows and Columns";
            public const string animSpeed = "Anim Speed";
            public const string normalMap = "Normal Map";
            public const string normalIntensity = "Normal Intensity";
            public const string color = "Color";
            public const string tiling = "Tiling";
            public const string movementType = "Movement Type";
            public const string speed = "Speed";
            public const string position = "Position";
            public const string percentage = "%";
            public const string renderDistance = "Render Distance";
            public const string rotation = "Rotation";
            public const string distortionValues = "Distortion Values";
            public const string intensity = "Intensity";
            public const string value = "Value";
            public const string size = "Size";
            public const string sharpness = "Sharpness";
            public const string skyColor = "Sky Color";
            public const string horizonColor = "Horizon Color";
            public const string groundColor = "Ground Color";
            public const string horizonHeight = "Horizon Height";
            public const string horizonWidth = "Horizon Width";
            public const string horizonMask = "Horizon Mask Values";
            public const string horizonMaskValues = "Horizon Mask Values";
            public const string horizonPlaneColor = "Horizon Plane Color";
            public const string horizonPlaneHeight = "Horizon Plane Height";
            public const string widthHeight = "Width & Height";
            public const string timeSpan = "Time Span";
            public const string start = "Start";
            public const string end = "End";
            public const string fade = "Fade";
            public const string inStr = "In";
            public const string outStr = "Out";
            public const string tint = "Tint";
            public const string max = "Max";
            public const string min = "Min";
            public const string offset = "Offset";
            public const string amount = "Amount";
            public const string spread = "Spread";
            public const string horizontal = "Horizontal";
            public const string vertical = "Vertical";
            public const string lightColor = "Light Color";
            public const string lightIntensity = "Light Intensity";
            public const string lightDirection = "Light Direction";
            public const string direction = "Direction";
            public const string trigger = "Trigger";
            public const string tintGradient = "Tint Gradient";
            public const string realtimeDLDirecn = "Realtime DL Direction";
            public const string ambientSkyColor = "Ambient Sky Color";
            public const string ambientEquatorColor = "Ambient Equator Color";
            public const string ambientGroundColor = "Ambient Ground Color";
            public const string fogColor = "Fog Color";
            public const string fogMode = "Fog Mode";
            public const string startDistance = "Start Distance";
            public const string endDistance = "End Distance";
            public const string density = "Density";
            public const string texture = "Texture";
        }

        // Short 
        public const string short_rotation = "Rot";
        public const string short_Hour = "Hr";

        //Satellite
        public static class Satellite3D
        {
            public const string PREFAB = "Prefab";
            public const string SIZE = "Size";
            public const string RADIUS = "Radius";
            public const string Y_OFFSET = "y-offset";
            public const string INITIAL_POS = "Initial Pos";
            public const string HORIZON_PLANE = "Horizon Plane";
            public const string INCLINATION = "Inclination";
            public const string ROTATION_TYPE = "Rotation Type";
        }

        public static class Planar3D
        {
            public const string SCENE = "Scene";

            public const string Y_POS = "Y-Pos";
            public const string FOLLOW_CAMERA = "Follow Camera";
            public const string RENDER_IN_MAIN_CAMERA = "Render In Main Camera";
        }
    }
}