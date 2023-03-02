using NUnit.Framework;
using UnityEditor.VersionControl;

public class ApplicationFocusServiceShould
{

    private ApplicationFocusService applicationFocusService;
    private bool applicationHasFocus;

    
    [SetUp]
    public void SetUp()
    {
        applicationFocusService = new ApplicationFocusService();
        applicationFocusService.Initialize();
    }

    [Test]
    public void FocusGained()
    {
        applicationHasFocus = false;
        applicationFocusService.OnApplicationFocus += ApplicationFocused;
        applicationFocusService.OnFocusGained();
        Assert.IsTrue(applicationHasFocus);
        applicationFocusService.OnApplicationFocus -= ApplicationFocused;
    }
    
    [Test]
    public void FocusLost()
    {
        applicationHasFocus = true;
        applicationFocusService.OnApplicationFocusLost += ApplicationLostFocus;
        applicationFocusService.OnFocusLost();
        Assert.IsFalse(applicationHasFocus);
        applicationFocusService.OnApplicationFocusLost -= ApplicationLostFocus;
    }
    
    private void ApplicationFocused()
    {
        applicationHasFocus = true;
    }
    
    private void ApplicationLostFocus()
    {
        applicationHasFocus = false;
    }

}
