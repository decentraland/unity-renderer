using NUnit.Framework;
using System.IO;
using System.Linq;
using UnityEngine;

[Category("EditModeCI")]
public class AddressablesValidationTests
{
    [TestCase("Scripts/MainScripts/DCL/Controllers/HUD/NotificationHUD")]
    public void ValidateFolderDoesNotHaveResourcesFolderInside(string folderName)
    {
        string folderPath = Application.dataPath + $"/{folderName}";
        var directory = new DirectoryInfo(folderPath);

        if (!directory.Exists)
            Assert.Fail($"{folderName} does not exist");

        bool hasResourcesFolder = directory.GetDirectories("*", SearchOption.AllDirectories).Any(subDirectory => subDirectory.Name == "Resources");
        Assert.IsFalse(hasResourcesFolder, $"{folderName} folder or its sub-folders contain Resources folder");
    }


}
