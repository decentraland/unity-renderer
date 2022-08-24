using System;
using System.Reflection;
using DCL.ECS7;
using NUnit.Framework;

namespace Tests
{
    public class InternalComponentsIdShould
    {
        [Test]
        public void NotCollideWithOtherIds()
        {
            FieldInfo[] fields = typeof(ComponentID).GetFields(BindingFlags.Public | BindingFlags.Static);

            for (int i = 0; i < fields.Length; i++)
            {
                var value = (int)fields[i].GetValue(null);
                Assert.IsFalse(Enum.IsDefined(typeof(InternalECSComponentsId), value));
            }
        }
    }
}