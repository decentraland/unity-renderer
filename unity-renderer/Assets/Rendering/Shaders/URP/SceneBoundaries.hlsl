#ifndef SCENE_BOUNDARIES_FILTER
#define SCENE_BOUNDARIES_FILTER
    // bool SceneBoundaries_IsInParcel(float3 position, half4 parcel)
    // {
    //     const half size = parcel.w;
    //     const half halfSize = size/2;
    //     const half2 center = half2(parcel.x*size, parcel.z*size) + half2(halfSize, halfSize); 
    //     if(position.x > (center.x+halfSize) || position.x < (center.x-halfSize)) return false;
    //     if(position.z > (center.y+halfSize) || position.z < (center.y-halfSize)) return false;
    //     return true;
    // }
    //
    // bool SceneBoundaries_IsInScene(float3 position)
    // {
    //     for(int i = 0 ; i < _ParcelAmount; i++)
    //     {
    //         if(SceneBoundaries_IsInParcel(position + half3(_GlobalOffset.x, 0, _GlobalOffset.z), _ParcelCenters[i] ))
    //             return true;
    //     }
    //     return false;
    // }
    //
    // #if defined(_ENABLE_SB)
    //     #define SCENE_BOUNDARIES_FILTER(position) {if(!SceneBoundaries_IsInScene(position)) discard;}  
    // #endif
    // #if defined(_ENABLE_SB_DEBUG)
    //     #define SCENE_BOUNDARIES_FILTER(position) {if(!SceneBoundaries_IsInScene(position)) return half4(1,0,0,1);}
    // #endif
#endif