using System;
using System.Threading;
using Altom.AltDriver.Commands;

namespace Altom.AltDriver
{
    public class AltObject
    {
        public string name;
        public int id;
        public int x;
        public int y;
        public int z;
        public int mobileY;
        public string type;
        public bool enabled;
        public float worldX;
        public float worldY;
        public float worldZ;
        public int idCamera;
        public int transformParentId;
        public int transformId;
        [Newtonsoft.Json.JsonIgnore]
        public IDriverCommunication CommHandler;

        public AltObject(string name, int id = 0, int x = 0, int y = 0, int z = 0, int mobileY = 0, string type = "", bool enabled = true, float worldX = 0, float worldY = 0, float worldZ = 0, int idCamera = 0, int transformParentId = 0, int transformId = 0)
        {
            this.name = name;
            this.id = id;
            this.x = x;
            this.y = y;
            this.z = z;
            this.mobileY = mobileY;
            this.type = type;
            this.enabled = enabled;
            this.worldX = worldX;
            this.worldY = worldY;
            this.worldZ = worldZ;
            this.idCamera = idCamera;
            this.transformParentId = transformParentId;
            this.transformId = transformId;
        }

        public AltObject getParent()
        {
            var altObject = new AltFindObject(CommHandler, By.PATH, "//*[@id=" + this.id + "]/..", By.NAME, "", true).Execute();
            CommHandler.SleepFor(CommHandler.GetDelayAfterCommand());
            return altObject;
        }

        public AltVector2 getScreenPosition()
        {
            return new AltVector2(x, y);
        }

        public AltVector3 getWorldPosition()
        {
            return new AltVector3(worldX, worldY, worldZ);
        }

        public T GetComponentProperty<T>(string componentName, string propertyName, string assemblyName, int maxDepth = 2)
        {
            var propertyValue = new AltGetComponentProperty<T>(CommHandler, componentName, propertyName, assemblyName, maxDepth, this).Execute();
            CommHandler.SleepFor(CommHandler.GetDelayAfterCommand());
            return propertyValue;
        }

        public void SetComponentProperty(string componentName, string propertyName, object value, string assemblyName)
        {
            new AltSetComponentProperty(CommHandler, componentName, propertyName, value, assemblyName, this).Execute();
            CommHandler.SleepFor(CommHandler.GetDelayAfterCommand());
        }

        public T CallComponentMethod<T>(string componentName, string methodName, string assemblyName, object[] parameters, string[] typeOfParameters = null)
        {
            var result = new AltCallComponentMethod<T>(CommHandler, componentName, methodName, parameters, typeOfParameters, assemblyName, this).Execute();
            CommHandler.SleepFor(CommHandler.GetDelayAfterCommand());
            return result;
        }

        public string GetText()
        {
            var text = new AltGetText(CommHandler, this).Execute();
            CommHandler.SleepFor(CommHandler.GetDelayAfterCommand());
            return text;
        }

        public AltObject SetText(string text, bool submit = false)
        {
            var altObject = new AltSetText(CommHandler, this, text, submit).Execute();
            CommHandler.SleepFor(CommHandler.GetDelayAfterCommand());
            return altObject;
        }

        /// <summary>
        /// Click current object
        /// </summary>
        /// <param name="count">Number of times to click</param>
        /// <param name="interval">Interval between clicks in seconds</param>
        /// <param name="wait">Wait for command to finish</param>
        /// <returns>The clicked object</returns>
        public AltObject Click(int count = 1, float interval = 0.1f, bool wait = true)
        {
            var altObject = new AltClickElement(CommHandler, this, count, interval, wait).Execute();
            CommHandler.SleepFor(CommHandler.GetDelayAfterCommand());
            return altObject;
        }

        public AltObject PointerUpFromObject()
        {
            var altObject = new AltPointerUpFromObject(CommHandler, this).Execute();
            CommHandler.SleepFor(CommHandler.GetDelayAfterCommand());
            return altObject;
        }

        public AltObject PointerDownFromObject()
        {
            var altObject = new AltPointerDownFromObject(CommHandler, this).Execute();
            CommHandler.SleepFor(CommHandler.GetDelayAfterCommand());
            return altObject;
        }

        public AltObject PointerEnterObject()
        {
            var altObject = new AltPointerEnterObject(CommHandler, this).Execute();
            CommHandler.SleepFor(CommHandler.GetDelayAfterCommand());
            return altObject;
        }

        public AltObject PointerExitObject()
        {
            var altObject = new AltPointerExitObject(CommHandler, this).Execute();
            CommHandler.SleepFor(CommHandler.GetDelayAfterCommand());
            return altObject;
        }

        /// <summary>
        /// Tap current object
        /// </summary>
        /// <param name="count">Number of taps</param>
        /// <param name="interval">Interval in seconds</param>
        /// <param name="wait">Wait for command to finish</param>
        /// <returns>The tapped object</returns>
        public AltObject Tap(int count = 1, float interval = 0.1f, bool wait = true)
        {
            var altObject = new AltTapElement(CommHandler, this, count, interval, wait).Execute();
            CommHandler.SleepFor(CommHandler.GetDelayAfterCommand());
            return altObject;
        }

        public System.Collections.Generic.List<AltComponent> GetAllComponents()
        {
            var altObject = new AltGetAllComponents(CommHandler, this).Execute();
            CommHandler.SleepFor(CommHandler.GetDelayAfterCommand());
            return altObject;
        }

        public System.Collections.Generic.List<AltProperty> GetAllProperties(AltComponent altComponent, AltPropertiesSelections altPropertiesSelections = AltPropertiesSelections.ALLPROPERTIES)
        {
            var altObject = new AltGetAllProperties(CommHandler, altComponent, this, altPropertiesSelections).Execute();
            CommHandler.SleepFor(CommHandler.GetDelayAfterCommand());
            return altObject;
        }

        public System.Collections.Generic.List<AltProperty> GetAllFields(AltComponent altComponent, AltFieldsSelections altFieldsSelections = AltFieldsSelections.ALLFIELDS)
        {
            var altObject = new AltGetAllFields(CommHandler, altComponent, this, altFieldsSelections).Execute();
            CommHandler.SleepFor(CommHandler.GetDelayAfterCommand());
            return altObject;
        }

        public System.Collections.Generic.List<string> GetAllMethods(AltComponent altComponent, AltMethodSelection methodSelection = AltMethodSelection.ALLMETHODS)
        {
            var altObject = new AltGetAllMethods(CommHandler, altComponent, methodSelection).Execute();
            CommHandler.SleepFor(CommHandler.GetDelayAfterCommand());
            return altObject;
        }
    }
}
