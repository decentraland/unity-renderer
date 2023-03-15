using UnityEngine;
using UnityEngine.UIElements;

namespace DCLServices.MapRendererV2.TestScene
{
    public class MapRendererTestSceneStyles
    {
        public const string FUNCTION_GROUP = "functionGroup";
        public const string GROUP_TITLE = "groupTitle";

        public static void SetStyleSheet(VisualElement visualElement)
        {
            visualElement.styleSheets.Add(Resources.Load<StyleSheet>("MapRendererTestScene"));
        }
    }
}
