using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL.Helpers
{
    public static class MaterialInfoUtils
    {
        private static IFormatProvider formatter;
        
        public static string ToText(this Material material)
        {
            if (formatter == null)
                formatter = new System.Globalization.CultureInfo("en-US");
            
            Shader shader = material.shader;
 
            string text = "";
 
            text += string.Format("Shader Name = {0}", shader.name);
            text += "\n";
            text += string.Format("Render Queue = {0}", shader.renderQueue);
            text += "\n";
 
            text += "Shader Keywords";
            text += "\n";
 
            for (int i = 0; i < material.shaderKeywords.Length; i++)
            {
                text += string.Format("{0}: {1}", i, material.shaderKeywords[i]);
                text += "\n";
            }
            text += "Properties";
            text += "\n";

            var propertyCount = shader.GetPropertyCount();
            for (int i = 0; i < propertyCount; i++)
            {
                string propertyName = shader.GetPropertyName(i);
                var propertyType = shader.GetPropertyType(i);
                string propertyDescription = shader.GetPropertyDescription(i);
 
                string value;
 
                switch (propertyType)
                {
                    case ShaderPropertyType.Color: // The property holds a Vector4 value representing a color.
                        value = string.Format(formatter, "{0}", material.GetColor(propertyName));
                        break;
                    case ShaderPropertyType.Vector: // The property holds a Vector4 value.
                        value = string.Format(formatter, "{0}", material.GetVector(propertyName));
                        break;
                    case ShaderPropertyType.Float: // The property holds a floating number value.
                        value = string.Format(formatter, "{0}", material.GetFloat(propertyName));
                        break;
                    case ShaderPropertyType.Range: //    The property holds a floating number value in a certain range.
                        value = string.Format(formatter, "{0}", material.GetFloat(propertyName));
                        break;
                    case ShaderPropertyType.Texture: // The property holds a Texture object.
                        value = material.GetTexture(propertyName) == null ? "null" : string.Format(formatter, "{0}", material.GetTexture(propertyName).dimension);
                        break;
                    default:
                        value = "<undefined>";
                        break;
                }
 
                text += string.Format("{0}: {1} = {2} ({3}, {4})", i, propertyName, value, propertyType, propertyDescription);
                text += "\n";
            }
 
            text += "Shader Passes";
            text += "\n";
            
            for (int i = 0; i < material.passCount; i++)
            {
                text += string.Format("{0}: {1} = {2}", i, material.GetPassName(i), material.GetShaderPassEnabled(material.GetPassName(i)));
                text += "\n";
            }

            return text;
        }
    }
}