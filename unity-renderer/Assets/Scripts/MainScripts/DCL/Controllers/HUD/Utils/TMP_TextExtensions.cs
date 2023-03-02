using TMPro;

public static class TMP_TextExtensions
{
    public static string ReplaceUnsupportedCharacters(this TMP_Text text, string bodyText, char replacementChar)
    {
        bool isBodyFormatSupported = text.font.HasCharacters(bodyText, out uint[] missing, searchFallbacks: true, tryAddCharacter: true);

        if (!isBodyFormatSupported)
        {
            if (missing != null && missing.Length > 0)
            {
                for (int i = 0; i < missing.Length; i++)
                {
                    bodyText = bodyText.Replace(((char)missing[i]), replacementChar);
                }
            }
        }

        return bodyText;
    }
}
