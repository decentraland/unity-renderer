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

            public bool IsAvailable { get; set; }
            public string Name { get; set; }
            public InputType InputType { get; set; }
            [CanBeNull] public string[] Values { get; set; }
            [CanBeNull] public string DateFormat { get; set; }

            protected bool Equals(Option other) =>
                IsAvailable == other.IsAvailable && Name == other.Name && InputType == other.InputType && Equals(Values, other.Values) && DateFormat == other.DateFormat;

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Option)obj);
            }

            public override int GetHashCode() =>
                HashCode.Combine(IsAvailable, Name, (int)InputType, Values, DateFormat);
        }

        public Dictionary<string, Option> Options { get; set; }

        protected bool Equals(AdditionalInfoOptionsModel other)
        {
            if (Options.Count != other.Options.Count)
                return false;

            foreach (string key in Options.Keys)
            {
                if (!other.Options.ContainsKey(key))
                    return false;

                if (!Options[key].Equals(other.Options[key]))
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AdditionalInfoOptionsModel)obj);
        }

        public override int GetHashCode() =>
            (Options != null ? Options.GetHashCode() : 0);
    }
}
