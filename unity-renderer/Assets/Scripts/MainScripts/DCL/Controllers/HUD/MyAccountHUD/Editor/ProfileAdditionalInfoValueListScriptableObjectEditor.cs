using System;
using UnityEditor;
using UnityEngine;

namespace DCL.MyAccount
{
    [CustomEditor(typeof(ProfileAdditionalInfoValueListScriptableObject))]
    public class ProfileAdditionalInfoValueListScriptableObjectEditor : Editor
    {
        private SerializedProperty valuesProperty;
        private string csvTextContent;

        private void OnEnable()
        {
            valuesProperty = serializedObject.FindProperty("values");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            csvTextContent = EditorGUILayout.TextArea(csvTextContent);

            if (GUILayout.Button("Generate from CSV"))
            {
                string[] values = ConvertCsvToStringArray(csvTextContent);

                valuesProperty.arraySize = values.Length;

                for (var i = 0; i < values.Length; i++)
                    valuesProperty.GetArrayElementAtIndex(i).stringValue = values[i];

                serializedObject.ApplyModifiedProperties();
            }
        }

        private string[] ConvertCsvToStringArray(string csvText) =>
            csvText.Split(new char[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries);
    }
}
