
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Altom.AltDriver;
using Altom.AltDriver.Commands;
using Altom.AltTester.Logging;
using Newtonsoft.Json;

namespace Altom.AltTester.Commands
{
    class AltReflectionMethodsCommand<TParam, TResult> : AltCommand<TParam, TResult> where TParam : CommandParams
    {
        private static readonly NLog.Logger logger = ServerLogManager.Instance.GetCurrentClassLogger();

        protected AltReflectionMethodsCommand(TParam cmdParams) : base(cmdParams) { }

        public Type GetType(string typeName, string assemblyName)
        {
            var type = Type.GetType(typeName);
            if (type != null)
                return type;
            if (string.IsNullOrEmpty(assemblyName))
            {
                if (typeName.Contains("."))
                {
                    assemblyName = typeName.Substring(0, typeName.LastIndexOf('.'));

                    var assembly = Assembly.Load(assemblyName);
                    if (assembly == null)
                        throw new ComponentNotFoundException("Component not found");
                    type = assembly.GetType(typeName);
                    if (type == null)
                        throw new ComponentNotFoundException("Component not found");
                    return type;
                }

                throw new ComponentNotFoundException("Component not found");
            }
            else
            {
                try
                {
                    var assembly = Assembly.Load(assemblyName);
                    if (assembly.GetType(typeName) == null)
                        throw new ComponentNotFoundException("Component not found");
                    return assembly.GetType(typeName);
                }
                catch (System.IO.FileNotFoundException)
                {
                    throw new AssemblyNotFoundException("Assembly not found");
                }
            }
        }

        public override TResult Execute()
        {
            throw new NotImplementedException();
        }

        protected MemberInfo GetMemberForObjectComponent(Type Type, string propertyName)
        {
            PropertyInfo propertyInfo = Type.GetProperty(propertyName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            FieldInfo fieldInfo = Type.GetField(propertyName,
                 BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (propertyInfo != null)
                return propertyInfo;
            if (fieldInfo != null)
                return fieldInfo;
            throw new PropertyNotFoundException("Property " + propertyName + " not found");
        }

        protected MethodInfo[] GetMethodInfoWithSpecificName(System.Type componentType, string altActionMethod)
        {
            MethodInfo[] methodInfos = componentType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Where(method => method.Name.Equals(altActionMethod)).ToArray();

            if (methodInfos.Length == 0)
            {
                throw new MethodNotFoundException(String.Format("Method {0} not found in {1}", altActionMethod, componentType.ToString()));
            }
            return methodInfos;
        }

        protected object InvokeMethod(MethodInfo methodInfo, string[] parameters, object component)
        {
            ParameterInfo[] parameterInfos = methodInfo.GetParameters();

            if (parameterInfos.Length != parameters.Length)
                throw new TargetParameterCountException();

            object[] parameterValues = new object[parameterInfos.Length];
            for (int i = 0; i < parameterInfos.Length; i++)
            {
                try
                {

                    parameterValues[i] = JsonConvert.DeserializeObject(parameters[i], parameterInfos[i].ParameterType,
                       new JsonSerializerSettings
                       {
                           Culture = CultureInfo.InvariantCulture
                       });
                }

                catch (Newtonsoft.Json.JsonException)
                {
                    throw new FailedToParseArgumentsException(string.Format("Could not parse parameter '{0}' to type {1}", parameters[i], parameterInfos[i].ParameterType));
                }
            }

            return methodInfo.Invoke(component, parameterValues);
        }

        protected object GetValueForMember(AltObject altObject, string[] fieldArray, Type componentType)
        {
            string propertyName;
            int index = getArrayIndex(fieldArray[0], out propertyName);
            MemberInfo memberInfo = GetMemberForObjectComponent(componentType, propertyName);
            var instance = AltRunner.GetGameObject(altObject).GetComponent(componentType);
            if (instance == null)
            {
                throw new ComponentNotFoundException("Component " + componentType.Name + " not found");
            }
            object value = GetValue(instance, memberInfo, index);

            for (int i = 1; i < fieldArray.Length && value != null; i++)
            {
                index = getArrayIndex(fieldArray[i], out propertyName);
                memberInfo = GetMemberForObjectComponent(value.GetType(), propertyName);
                value = GetValue(value, memberInfo, index);
            }
            return value;
        }


        protected string SetValueForMember(AltObject altObject, string[] fieldArray, Type componentType, string valueString)
        {
            var instance = AltRunner.GetGameObject(altObject).GetComponent(componentType);
            if (instance == null)
            {
                throw new ComponentNotFoundException("Component " + componentType.Name + " not found");
            }
            setValueRecursive(valueString, fieldArray, instance);

            return "valueSet";
        }
        private object setValueRecursive(string valueAsString, string[] fieldArray, object instance, int counter = 0)
        {
            string propertyName;
            int index = getArrayIndex(fieldArray[counter], out propertyName);
            MemberInfo memberInfo = GetMemberForObjectComponent(instance.GetType(), propertyName);
            object value = GetValue(instance, memberInfo, index);
            if (counter < fieldArray.Length - 1)
            {
                counter++;
                var valueObtained = setValueRecursive(valueAsString, fieldArray, value, counter);
                if (index == -1)
                {
                    if (memberInfo.GetType().Equals(typeof(PropertyInfo)))
                    {
                        ((PropertyInfo)memberInfo).SetValue(instance, valueObtained);
                    }
                    else
                    {
                        ((FieldInfo)memberInfo).SetValue(instance, valueObtained);

                    }
                }

                return value;
            }
            else
            {
                setValue(instance, memberInfo, index, valueAsString, value.GetType());
                return instance;
            }
        }

        protected object GetInstance(object instance, string[] methodPathSplited, Type componentType = null)
        {
            return getInstance(instance, methodPathSplited, 0, componentType);
        }

        protected object GetValue(object instance, MemberInfo memberInfo, int index)
        {
            object value = null;
            if (memberInfo.MemberType == MemberTypes.Property)
            {
                value = ((PropertyInfo)memberInfo).GetValue(instance, null);
            }
            else if (memberInfo.MemberType == MemberTypes.Field)
            {
                value = ((FieldInfo)memberInfo).GetValue(instance);
            }
            if (index == -1)
            {
                return value;
            }
            else
            {
                System.Collections.IEnumerable enumerable = value as System.Collections.IEnumerable;
                if (enumerable != null)
                {
                    int i = 0;
                    foreach (object element in enumerable)
                    {
                        if (i == index)
                        {
                            return element;
                        }
                        i++;
                    }
                }
                throw new AltException(AltErrors.errorIndexOutOfRange);
            }
        }

        private int getArrayIndex(string arrayProperty, out string propertyName)
        {
            if (Regex.IsMatch(arrayProperty, @".*\[[0-9]\]*"))
            {
                var arrayPropertySplited = arrayProperty.Split('[');
                propertyName = arrayPropertySplited[0];
                return int.Parse(arrayPropertySplited[1].Split(']')[0]);
            }
            else
            {
                propertyName = arrayProperty;
                return -1;
            }
        }

        private void setValue(object instance, MemberInfo memberInfo, int index, string valueString, Type valueType)
        {
            if (memberInfo.MemberType == MemberTypes.Property)
            {
                PropertyInfo propertyInfo = (PropertyInfo)memberInfo;

                if (index != -1)
                {
                    object value = deserializeMemberValue(valueString, valueType);
                    var listValue = GetValue(instance, memberInfo, -1);

                    SetElementOfListObject(index, listValue, value);
                    propertyInfo.SetValue(instance, listValue);

                }
                else
                {
                    object value = deserializeMemberValue(valueString, valueType);
                    propertyInfo.SetValue(instance, value);
                }
            }
            else if (memberInfo.MemberType == MemberTypes.Field)
            {
                FieldInfo fieldInfo = (FieldInfo)memberInfo;

                if (index != -1)
                {
                    object value = deserializeMemberValue(valueString, valueType);
                    var listValue = GetValue(instance, memberInfo, -1);

                    SetElementOfListObject(index, listValue, value);
                    fieldInfo.SetValue(instance, listValue);
                }
                else
                {
                    object value = deserializeMemberValue(valueString, valueType);
                    fieldInfo.SetValue(instance, value);
                }
            }
        }

        public object SetElementOfListObject(int index, object enumerable, object value)
        {
            var list = (IList)enumerable;
            if (list != null)
                list[index] = value;
            return list;
            throw new AltException(AltErrors.errorPropertyNotSet);
        }

        private object deserializeMemberValue(string valueString, System.Type type)
        {
            object value;
            try
            {
                value = Newtonsoft.Json.JsonConvert.DeserializeObject(valueString, type);
            }
            catch (Newtonsoft.Json.JsonException e)
            {
                logger.Trace(e);
                value = null;
            }
            return value;
        }

        private object getInstance(object instance, string[] methodPathSplited, int index, Type componentType = null)
        {
            if (methodPathSplited.Length - 1 <= index)
                return instance;
            string propertyName;
            int indexValue = getArrayIndex(methodPathSplited[index], out propertyName);

            Type type = instance == null ? componentType : instance.GetType();//Checking for static fields

            MemberInfo memberInfo = GetMemberForObjectComponent(type, propertyName);
            instance = GetValue(instance, memberInfo, indexValue);
            if (instance == null)
            {
                string path = "";
                for (int i = 0; i < index; i++)
                {
                    path += methodPathSplited[i] + ".";
                }
                throw new Altom.AltDriver.NullReferenceException(path + propertyName + "is not assigned");
            }
            index++;
            return getInstance(instance, methodPathSplited, index);
        }

    }
}
