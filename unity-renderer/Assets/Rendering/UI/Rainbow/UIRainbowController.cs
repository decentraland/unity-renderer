using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRainbowController : MonoBehaviour
{
    Material targetMat;
    public Image img;
    public Texture mask;
    public bool useTexture;
    public Texture gradientTexture;
    public Gradient gradient;
    public Vector2 speed;
    [Range(-360, 360)]
    public float rotation;
    [Range(0,1)]
    public float fill = 1f;

    public enum rainbowT {linear, radial};
    public enum fillT {left, right, up, down};

    public rainbowT rainbowType;
    public fillT fillType;

    void Start()
    {
        if(img.maskable)
        {
            targetMat = img.materialForRendering;
        }
        else
        {
            targetMat = new Material(img.material);
            img.material = targetMat;
        }
    }


    void FixedUpdate()
    {
        if(targetMat)
        {
            SetValues();
        }
        
    }

    void SetValues()
    {
        targetMat.SetTexture("_Mask", mask);
        targetMat.SetTexture("_Ramp", gradientTexture);
        targetMat.SetVector("_Speed", speed);
        targetMat.SetFloat("_Rotation", rotation);
        targetMat.SetFloat("_Fill", fill);

        if (useTexture)
        {
            targetMat.SetFloat("_UseTexture", 1);
        }
        else
        {
            targetMat.SetFloat("_UseTexture", 0);
        }

        targetMat.SetFloat("_GradientMode", ((int)rainbowType));
        targetMat.SetFloat("_FillDirection", ((int)fillType));

        targetMat.SetFloat("_ColorAmount", gradient.colorKeys.Length);

        float[] tempPos01 = new float[4];
        float[] tempPos02 = new float[4];

        for (int i = 0; i < 4; i++)
        {
            tempPos01[i] = 1;
            tempPos02[i] = 1;
        }

        for (int i = 0; i < gradient.colorKeys.Length; i++)
        {
            targetMat.SetColor("_Color0" + (i + 1).ToString(), gradient.colorKeys[i].color);
            if (i <= 3)
            {
                tempPos01[i] = gradient.colorKeys[i].time;
            }
            else
            {
                tempPos02[i - 4] = gradient.colorKeys[i].time;
            }
        }
        Vector4 pos01 = new Vector4(tempPos01[0], tempPos01[1], tempPos01[2], tempPos01[3]);
        Vector4 pos02 = new Vector4(tempPos02[0], tempPos02[1], tempPos02[2], tempPos02[3]);

        targetMat.SetVector("_GradientPositions01", pos01);
        targetMat.SetVector("_GradientPositions02", pos02);
    }
}
