using Unity.PerformanceTesting;
using NUnit.Framework;
using DCL;
using DCL.Interface;
using DCL.Models;

namespace MessaginPerformanceTests
{
    public class MessageDecodingTests
    {
        public void SetupTests()
        {
        }

        [Test, Performance]
        public void MeasureTimeToDecodeMessages()
        {
            string[] messages = GetMessagesFromFile(MessageDecoder.MESSAGES_FILENAME);
            int count = messages.Length;
            string sceneId;
            string tag;
            string message;
            PB_SendSceneMessage sendSceneMessage;

            Measure.Method(() =>
                {
                    for (var i = 0; i < count; i++)
                    {
                        MessageDecoder.DecodePayloadChunk(messages[i], out sceneId, out message, out tag, out sendSceneMessage);
                    }
                })
                .SetUp(() => SetupTests())
                .WarmupCount(3)
                .MeasurementCount(10)
                .IterationsPerMeasurement(10)
                .GC()
                .Run();
        }

        [Test, Performance]
        public void MeasureTimeToDecodeTransform()
        {
            string[] messages = GetMessagesFromFile(MessageDecoder.TRANSFORM_FILENAME);
            int count = messages.Length;
            DCL.Components.DCLTransform.Model transformModel = new DCL.Components.DCLTransform.Model();

            Measure.Method(() =>
                {
                    for (var i = 0; i < count; i++)
                    {
                        MessageDecoder.DecodeTransform(messages[i], ref transformModel);
                    }
                })
                .SetUp(() => SetupTests())
                .WarmupCount(3)
                .MeasurementCount(10)
                .IterationsPerMeasurement(10)
                .GC()
                .Run();
        }

        [Test, Performance]
        public void MeasureTimeToDecodeQuery()
        {
            string[] messages = GetMessagesFromFile(MessageDecoder.QUERY_FILENAME);
            int count = messages.Length;
            QueryMessage queryMessage = new QueryMessage();

            Measure.Method(() =>
                {
                    for (var i = 0; i < count; i++)
                    {
                        MessageDecoder.DecodeQueryMessage("raycast", messages[i], ref queryMessage);
                    }
                })
                .SetUp(() => SetupTests())
                .WarmupCount(3)
                .MeasurementCount(10)
                .IterationsPerMeasurement(10)
                .GC()
                .Run();
        }

        public string[] GetMessagesFromFile(string filename)
        {
            string fullFilename = System.IO.Path.Combine(MessageDecoder.DUMP_PATH, filename);
            string text = System.IO.File.ReadAllText(fullFilename);

            return text.Split(new char[] {'\n'}, System.StringSplitOptions.RemoveEmptyEntries);
        }
    }
}