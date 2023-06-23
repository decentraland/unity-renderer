using System;
using System.Collections.Generic;

namespace DCL.MyAccount
{
    public class AdditionalInfoOptionsModel
    {
        public class Option
        {
            public Action<string> OnValueSubmitted;
            public Action OnRemoved;

            public string Name { get; set; }
        }

        public IEnumerable<Option> Options { get; set; }
    }
}
