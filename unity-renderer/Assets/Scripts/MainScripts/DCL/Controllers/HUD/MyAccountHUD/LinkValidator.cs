using System.Text.RegularExpressions;

namespace DCL.MyAccount
{
    /// <summary>
    /// A helper class to validate urls. These must start either with http or https.
    /// </summary>
    public static class LinkValidator
    {
        private static readonly Regex httpRegex = new (@"^(?:https?):\/\/[^\s\/$.?#].[^\s]*$");

        /// <summary>
        /// Validates a given url checking if it starts with http or https.
        /// </summary>
        /// <param name="url">The url to validate.</param>
        /// <returns>Whether the url is valid or not.</returns>
        public static bool IsValid(string url) => httpRegex.IsMatch(url);
    }
}
