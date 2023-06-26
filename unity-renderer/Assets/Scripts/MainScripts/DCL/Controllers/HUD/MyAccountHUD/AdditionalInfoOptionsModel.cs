using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace DCL.MyAccount
{
    public class AdditionalInfoOptionsModel
    {
        public enum InputType
        {
            FreeFormText,
            StrictValueList,
            Date
        }

        public class Option
        {
            public Action<string> OnValueSubmitted;
            public Action OnRemoved;

            public string Name { get; set; }
            public InputType InputType { get; set; }
            [CanBeNull] public string[] Values { get; set; }
            [CanBeNull] public string DateFormat { get; set; }
        }

        public IEnumerable<Option> Options { get; set; }
    }
}
