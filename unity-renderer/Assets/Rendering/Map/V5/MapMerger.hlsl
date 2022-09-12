void MapMerger_float(float4 MapMask, float OutlineSobel, float OutlineRegular, float Highlight, float HighlightInner, float HighlightMid, float GridOpacity,
                     float4 ColorPlaza, float4 ColorEstates, float4 ColorStreets, float4 ColorBase, float4 ColorBackground, float4 ColorGrid, float4 ColorHighlight, float4 ColorHighlightInner, float4 ColorHighlightMid,
                     out float4 Out)
{
    Out = lerp(ColorPlaza, ColorEstates, MapMask.r);
    Out = lerp(Out, ColorStreets, MapMask.g);
    Out = lerp(Out, ColorBase, MapMask.b);
    Out = lerp(Out, ColorBackground, MapMask.a);

    //float tempGrid = ((1 - OutlineSobel) * OutlineRegular) * GridOpacity;
    //Out = Out + tempGrid;
    // 
    float tempGrid = ((1 - OutlineSobel) * OutlineRegular);
    Out = lerp(Out, float4(0,0,0,0), tempGrid * GridOpacity);
    
    
    Out = lerp(Out, ColorGrid, ColorGrid.a * OutlineSobel);

    Out = lerp(Out, ColorHighlight, ColorHighlight.a * Highlight);
    Out = lerp(Out, ColorHighlightInner, HighlightInner);
    Out = lerp(Out, ColorHighlightMid, HighlightMid);
}