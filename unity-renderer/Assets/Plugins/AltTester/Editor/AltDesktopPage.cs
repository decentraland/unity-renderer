namespace Altom.AltTesterEditor
{
    public class AltDesktopPage : UnityEditor.EditorWindow
    {
        public static AltDesktopPage _window;
        //TODO change image to gif once we have a final gif
        public static UnityEngine.Texture2D image;
        private readonly string titleOfPage = "<b><size=16>AltTester Desktop is a desktop app which can help you visualize the game objects hierarchy and get all the properties easily.</size></b>";
        private readonly string contentText = "<b>Main features </b> \n    • get object’s components, assemblies, methods, fields and properties without accessing the source code \n    • interact with your game from AltTester Desktop using keyboard, mouse, touchscreen and joystick actions \n    • load any scene or level\n    • control the speed of the game for debugging and test design purposes\n    • verify if a selector is correct and highlight the matching objects";
        private readonly string buttonText = "<b><size=16>Start free trial</size></b>";
        private UnityEngine.Vector2 scrollPos;
        private UnityEngine.GUIStyle gUIStyleText;
        private UnityEngine.GUIStyle gUIStyleButton;
        private UnityEngine.GUIStyle guiStylePadding;
        private static UnityEngine.Font font;

        private bool isProfessional;

        private float MaximumSizeForOneColumnLayout = 500;
        private float MaximumSizeForTitleToNotBeInFirstColumn = 1000;


        private UnityEngine.Texture2D buttonTexture;
        [UnityEditor.MenuItem("AltTester/AltTester Desktop", false, 81)]
        public static void ShowWindow()
        {
            _window = (AltDesktopPage)GetWindow(typeof(AltDesktopPage));
            _window.minSize = new UnityEngine.Vector2(300, 300);

            UnityEngine.GUIContent titleContent = new UnityEngine.GUIContent("AltTester Desktop");
            _window.titleContent = titleContent;
            _window.Show();
        }
        private void OnFocus()
        {
            if (buttonTexture == null)
            {
                buttonTexture = AltTesterEditorWindow.MakeTexture(20, 20, new UnityEngine.Color(0.07058824f, 0.6039216f, 0.145098f, 1f));
            }
            if (image == null)
            {
                var findImage = UnityEditor.AssetDatabase.FindAssets("AltDesktopPNG");
                image = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(UnityEditor.AssetDatabase.GUIDToAssetPath(findImage[0]));
            }
        }
        private void OnGUI()
        {
            if (isProfessional != UnityEditor.EditorGUIUtility.isProSkin)
            {
                isProfessional = UnityEditor.EditorGUIUtility.isProSkin;
                gUIStyleText = null;
            }
            if (guiStylePadding == null)
            {
                guiStylePadding = new UnityEngine.GUIStyle();
                guiStylePadding.padding = new UnityEngine.RectOffset(15, 15, 15, 15);
            }
            if (font == null)
            {
                font = UnityEngine.Font.CreateDynamicFontFromOSFont("Arial", 18);
            }
            if (gUIStyleButton == null)
            {
                gUIStyleButton = new UnityEngine.GUIStyle(UnityEngine.GUI.skin.button)
                {
                    wordWrap = true,
                    richText = true,
                    alignment = UnityEngine.TextAnchor.MiddleCenter,
                    font = font
                };
                gUIStyleButton.normal.background = buttonTexture;
                gUIStyleButton.normal.textColor = UnityEngine.Color.white;
            }
            if (gUIStyleText == null)
            {
                gUIStyleText = new UnityEngine.GUIStyle(UnityEngine.GUI.skin.label)
                {
                    wordWrap = true,
                    richText = true,
                    alignment = UnityEngine.TextAnchor.MiddleLeft,
                    font = font
                };
            }

            var screenWidth = UnityEditor.EditorGUIUtility.currentViewWidth;

            UnityEditor.EditorGUILayout.BeginVertical(guiStylePadding);
            scrollPos = UnityEditor.EditorGUILayout.BeginScrollView(scrollPos, false, false);

            if (screenWidth < MaximumSizeForTitleToNotBeInFirstColumn)
            {
                displayTitle();
                UnityEditor.EditorGUILayout.Separator();
                UnityEditor.EditorGUILayout.Separator();
            }
            if (screenWidth > MaximumSizeForOneColumnLayout)
            {
                DisplayContentOnTwoColumn();
            }
            else
            {
                DisplayContentOnOneColumn();
            }
            UnityEditor.EditorGUILayout.EndScrollView();
            UnityEditor.EditorGUILayout.EndVertical();
        }

        private void DisplayContentOnOneColumn()
        {
            float imageWidth, imageHeight;
            CalculateImageSize(out imageWidth, out imageHeight);

            displayContentText();
            UnityEditor.EditorGUILayout.Separator();

            displayImage(imageWidth, imageHeight);
            UnityEditor.EditorGUILayout.Separator();

            displayButton();
            UnityEditor.EditorGUILayout.Space();


        }
        private void DisplayContentOnTwoColumn()
        {

            float imageWidth, imageHeight;
            CalculateImageSize(out imageWidth, out imageHeight);

            float firstColumnHeight = CalculateFirstColumnHeight();
            float firstColumnSpace = 10f;
            float secondColumnSpace = 0;
            if (firstColumnHeight > imageHeight)
            {
                secondColumnSpace = (firstColumnHeight - imageHeight) * 0.5f;
            }

            UnityEditor.EditorGUILayout.BeginHorizontal();
            //first column
            UnityEditor.EditorGUILayout.BeginVertical(UnityEngine.GUILayout.ExpandHeight(true));

            if (position.width > MaximumSizeForTitleToNotBeInFirstColumn)
            {
                displayTitle();
                UnityEngine.GUILayout.Space(firstColumnSpace);

            }
            UnityEngine.GUILayout.Space(secondColumnSpace);
            displayContentText();
            UnityEditor.EditorGUILayout.Separator();
            displayButton();
            UnityEditor.EditorGUILayout.EndVertical();
            //second column
            UnityEditor.EditorGUILayout.BeginVertical(UnityEngine.GUILayout.ExpandHeight(true));
            UnityEngine.GUILayout.Space(secondColumnSpace);
            displayImage(imageWidth, imageHeight);
            UnityEditor.EditorGUILayout.EndVertical();
            UnityEditor.EditorGUILayout.EndHorizontal();
        }

        private float CalculateFirstColumnHeight()
        {
            var width = position.width / 2;
            float textHeight = gUIStyleText.CalcHeight(new UnityEngine.GUIContent(contentText), width);
            float titleHeight = 0;
            if (position.width > MaximumSizeForTitleToNotBeInFirstColumn)
            {
                titleHeight = gUIStyleText.CalcHeight(new UnityEngine.GUIContent(titleOfPage), width);
            }
            return textHeight + titleHeight;
        }

        private void displayTitle()
        {
            UnityEngine.GUILayout.Label(titleOfPage, gUIStyleText);
        }
        private void displayContentText()
        {
            UnityEngine.GUILayout.Label(contentText, gUIStyleText);
        }
        private void displayButton()
        {
            UnityEditor.EditorGUILayout.BeginHorizontal();


            if (UnityEngine.GUILayout.Button(buttonText, gUIStyleButton, UnityEngine.GUILayout.Width(150), UnityEngine.GUILayout.Height(50)))
            {
                downloadDesktop();
            }
            UnityEditor.EditorGUILayout.Space();
            UnityEditor.EditorGUILayout.EndHorizontal();
        }
        private void displayImage(float width, float height)
        {
            var rect = UnityEditor.EditorGUILayout.GetControlRect(UnityEngine.GUILayout.Width(width), UnityEngine.GUILayout.Height(height));
            UnityEditor.EditorGUI.DrawPreviewTexture(rect, image);
        }

        private void CalculateImageSize(out float screenWidth, out float screenHeight)
        {
            screenWidth = position.width - 60;
            if (screenWidth > MaximumSizeForOneColumnLayout - 60)
            {
                screenWidth /= 2;
            }
            screenHeight = position.height;
            ResizeImage(ref screenWidth, ref screenHeight, image.width, image.height);
        }

        private void downloadDesktop()
        {
            UnityEngine.Application.OpenURL("https://altom.com/testing-tools/alttester/#pricing");
        }

        private void ResizeImage(ref float maxWidth, ref float maxHeight, float imageWidth, float imageHeight)
        {
            float aspect = imageWidth / imageHeight;
            if (imageWidth > maxWidth)
            {
                imageWidth = maxWidth;
                imageHeight = imageWidth / aspect;
            }
            if (imageHeight > maxHeight)
            {
                aspect = imageWidth / imageHeight;
                imageHeight = maxHeight;
                imageWidth = imageHeight * aspect;
            }
            maxWidth = imageWidth;
            maxHeight = imageHeight;
        }
    }

}