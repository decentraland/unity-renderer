using System.Text;
using Unity.Profiling;
using UnityEngine;

namespace DefaultNamespace
{
    public class ExampleProfilerScript: MonoBehaviour
    {
        string statsText;
        ProfilerRecorder systemMemoryRecorder;
        ProfilerRecorder gcMemoryRecorder;
        ProfilerRecorder mainThreadTimeRecorder;

        static double GetRecorderFrameAverage(ProfilerRecorder recorder)
        {
            int samplesCount = recorder.Capacity;
            if (samplesCount == 0)
                return 0;

            double r = 0;

            unsafe
            {
                var samples = stackalloc ProfilerRecorderSample[samplesCount];
                recorder.CopyTo(samples, samplesCount);
                for (var i = 0; i < samplesCount; ++i)
                    r += samples[i].Value;
                r /= samplesCount;
            }

            return r;
        }

        void OnEnable()
        {
            systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
            gcMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
            mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
        }

        void OnDisable()
        {
            systemMemoryRecorder.Dispose();
            gcMemoryRecorder.Dispose();
            mainThreadTimeRecorder.Dispose();
        }

        void Update()
        {
            double frameTime = GetRecorderFrameAverage(mainThreadTimeRecorder) * (1e-6f);
            double fps = 1000 / frameTime;

            var sb = new StringBuilder(500);
            sb.AppendLine($"Frame Time Raw: {mainThreadTimeRecorder.LastValue* (1e-6f):F1} ms");
            sb.AppendLine($"Frame Time: {frameTime:F1} ms");
            sb.AppendLine($"FPS Raw: {1000 / (mainThreadTimeRecorder.LastValue* (1e-6f)):F1} ms");
            sb.AppendLine($"FPS: {fps:F1} ms");


            sb.AppendLine($"GC Memory: {gcMemoryRecorder.LastValue / (1024 * 1024)} MB");
            sb.AppendLine($"System Memory: {systemMemoryRecorder.LastValue / (1024 * 1024)} MB");
            statsText = sb.ToString();
        }

        void OnGUI()
        {
            GUI.skin.label.fontSize = 45;
            GUI.TextArea(new Rect(10, 30, 250, 250), statsText);
            GUI.skin.label.fontSize = 45;
        }
    }
}
