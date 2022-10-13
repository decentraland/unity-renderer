using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterpolatorControl : MonoBehaviour
{

    [SerializeField] private Color fogColor;
    
    [SerializeField] private Color[] infiniteFloorFogColors;
    [SerializeField] private Color infiniteFloorFogColor;
    
    [SerializeField] private Material roundEdgesInterpolatorMat;
    
    [SerializeField] private GameObject infiniteFloor;

    private void Awake()
    {
        roundEdgesInterpolatorMat = transform.GetComponent<SkinnedMeshRenderer>().material;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetFogColor();
        //SetFogColor();
        
        //infiniteFloorFogColor = GetVertexColor(1);
        //infiniteFloorFogColors = GetInfiniteFloorColors();

        //infiniteFloorFogColor = infiniteFloorFogColors[1];

    }
    
    private void GetFogColor()
    {
        fogColor = RenderSettings.fogColor;
    }
    
    private void SetFogColor()
    {
        roundEdgesInterpolatorMat.SetColor("_FogColor", fogColor);
    }
    
    // get color from an objects vertex
    private Color GetVertexColor(int vertexIndex)
    {
        Color[] colors = new Color[transform.GetComponent<SkinnedMeshRenderer>().sharedMesh.vertexCount];
        //transform.GetComponent<SkinnedMeshRenderer>().sharedMesh.colors = colors;
        //return colors[vertexIndex];
        
        // get mesh colors from infiniteFloor
        Color[] infiniteFloorColors = infiniteFloor.GetComponent<MeshRenderer>().sharedMaterial.GetColorArray("_Colors");
        
        // return
        return infiniteFloorColors[vertexIndex];
        
    }
    
    // get colors of infinite floor
    private Color[] GetInfiniteFloorColors()
    {
        Color[] infiniteFloorColors = infiniteFloor.GetComponent<MeshRenderer>().sharedMaterial.GetColorArray("_Colors");
        Debug.Log("infiniteFloorColors: " + infiniteFloorColors);
        return infiniteFloorColors;
    }
}
