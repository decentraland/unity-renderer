using UnityEngine;

namespace DCL.SettingsCommon
{
    [CreateAssetMenu(fileName = "QualitySettings", menuName = "QualitySettings")]
    public class QualitySettingsData : ScriptableObject
    {
        [SerializeField] int defaultPresetIndex = 0;
        [SerializeField] QualitySettings[] settings = null;

        public QualitySettings this[int i] { get { return settings[i]; } }

        public int Length { get { return settings.Length; } }

        public QualitySettings defaultPreset { get { return settings[defaultPresetIndex]; } }

        public int defaultIndex { get { return defaultPresetIndex; } }

        public void Set(QualitySettings[] newSettings) { settings = newSettings; }
    }

}