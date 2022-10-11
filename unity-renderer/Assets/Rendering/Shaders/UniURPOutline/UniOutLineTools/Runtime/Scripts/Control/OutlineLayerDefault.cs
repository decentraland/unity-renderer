using System;

namespace UniOutline.Outline
{
	
    // Enumerates outline render modes.
	
    [Flags]
    public enum OutlineLayerDefault
    {
        // the Layers
        Default = 0,
        TransparentFX = 1,
        IgnoreRaycast = 2,
        Water = 4,
        UI = 5,
        
        PostProcessing = 8,
        OnPointerEvent = 9,
        Ground = 10,
        Gizmo = 11,
        Selection = 12,
        Minimap = 13,
        OnBuilderPointerClick = 14,
        CharacterPreview = 15,
        CharacterController = 16,
        CharacterOnly = 17,
        NavmapChunk = 18,
        FriendsHUDPlayerMenu = 19,
        PlayerInfoCardMenu = 20,
        
        AvatarTriggerDetection = 21,
        Tutorial = 22,
        
        ViewportCullingIgnored = 23,
        FX = 24,
        
        Skybox = 25,
        
        Outlined = 26,
        
        OutlinedOccluded = 27,
        OutlinedBlurred = 28
        
    }
}