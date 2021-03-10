using DCL;

public class SceneMessageUtilities
{
    public static bool DecodePayloadChunk(string chunk, out string sceneId, out string message, out string tag)
    {

        sceneId = message = tag = null;

        var separatorPosition = chunk.IndexOf('\t');

        if (separatorPosition == -1)
        {
            return false;
        }

        sceneId = chunk.Substring(0, separatorPosition);

        var lastPosition = separatorPosition + 1;
        separatorPosition = chunk.IndexOf('\t', lastPosition);

        message = chunk.Substring(lastPosition, separatorPosition - lastPosition);
        lastPosition = separatorPosition + 1;

        separatorPosition = chunk.IndexOf('\t', lastPosition);

        message += '\t' + chunk.Substring(lastPosition, separatorPosition - lastPosition);
        lastPosition = separatorPosition + 1;

        tag = chunk.Substring(lastPosition);

        return true;
    }

    public static QueuedSceneMessage_Scene DecodeSceneMessage(string sceneId, string message, string tag)
    {
        var queuedMessage = new QueuedSceneMessage_Scene()
        { type = QueuedSceneMessage.Type.SCENE_MESSAGE, sceneId = sceneId, message = message, tag = tag };

        var queuedMessageSeparatorIndex = queuedMessage.message.IndexOf('\t');

        queuedMessage.method = queuedMessage.message.Substring(0, queuedMessageSeparatorIndex);

        return queuedMessage;
    }
}
