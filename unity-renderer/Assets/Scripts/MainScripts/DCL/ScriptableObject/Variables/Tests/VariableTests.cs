using UnityEngine;
using UnityEngine.Assertions;

namespace VariableTests
{
    public class BaseVariableTest<ValueType, VariableType>
        where VariableType : BaseVariableAsset<ValueType>
    {
        protected void VariableValueIsChangedProperly(ValueType value1, ValueType value2)
        {
            var variable = ScriptableObject.CreateInstance<VariableType>();

            variable.Set(value1);

            Assert.IsTrue(variable.Equals(value1));

            variable.Set(value2);

            Assert.IsTrue(variable.Equals(value2));
        }

        protected void ShouldNotCallOnChangedWhenSameValueIsSet(ValueType value1, ValueType value2)
        {
            var variable = ScriptableObject.CreateInstance<VariableType>();

            bool onChangeWasCalled = false;

            BaseVariableAsset<ValueType>.Change onChange = (x, y) => { onChangeWasCalled = true; };

            variable.OnChange += onChange;

            variable.Set(value1);
            variable.Set(value1);

            Assert.IsFalse(onChangeWasCalled);

            variable.Set(value2);

            Assert.IsTrue(onChangeWasCalled);

            variable.OnChange -= onChange;
        }
    }
    public class FloatVariableShould : BaseVariableTest<float, FloatVariable>
    {
        [NUnit.Framework.Test]
        public void NotCallOnChangedWhenSameValueIsSet()
        {
            base.ShouldNotCallOnChangedWhenSameValueIsSet(0, 1);
        }

        [NUnit.Framework.Test]
        public void VariableValueIsChangedProperly()
        {
            base.VariableValueIsChangedProperly(0, 1);
        }
    }

    public class Vector2IntVariableShould : BaseVariableTest<Vector2Int, Vector2IntVariable>
    {
        [NUnit.Framework.Test]
        public void NotCallOnChangedWhenSameValueIsSet()
        {
            base.ShouldNotCallOnChangedWhenSameValueIsSet(new Vector2Int(0, 0), new Vector2Int(1, 1));
        }

        [NUnit.Framework.Test]
        public void VariableValueIsChangedProperly()
        {
            base.VariableValueIsChangedProperly(new Vector2Int(0, 0), new Vector2Int(1, 1));
        }
    }

    public class Vector3VariableShould : BaseVariableTest<Vector3, Vector3Variable>
    {
        [NUnit.Framework.Test]
        public void NotCallOnChangedWhenSameValueIsSet()
        {
            base.ShouldNotCallOnChangedWhenSameValueIsSet(new Vector3(0, 0, 0), new Vector3(1, 1, 1));
        }

        [NUnit.Framework.Test]
        public void VariableValueIsChangedProperly()
        {
            base.VariableValueIsChangedProperly(new Vector3(0, 0, 0), new Vector3(1, 1, 1));
        }
    }
}
