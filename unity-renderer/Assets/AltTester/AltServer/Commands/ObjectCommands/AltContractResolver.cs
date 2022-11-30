using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Altom.AltTester.Commands
{
    public class AltContractResolver : DefaultContractResolver
    {
        private readonly Func<bool> _includeProperty;

        public AltContractResolver(Func<bool> includeProperty)
        {
            this._includeProperty = includeProperty;
        }


        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (property.AttributeProvider.GetAttributes(true).OfType<ObsoleteAttribute>().Any())
            {
                property.ShouldSerialize = obj => false;
            }
            var shouldSerialize = property.ShouldSerialize;
            property.ShouldSerialize = obj => _includeProperty() &&
                                              (shouldSerialize == null ||
                                               shouldSerialize(obj));
            return property;
        }
    }

    public class CustomJsonTextWriter : JsonTextWriter
    {
        public CustomJsonTextWriter(System.IO.TextWriter textWriter) : base(textWriter) { }

        public int CurrentDepth { get; private set; }

        public override void WriteStartObject()
        {
            CurrentDepth++;
            base.WriteStartObject();
        }

        public override void WriteEndObject()
        {
            CurrentDepth--;
            base.WriteEndObject();
        }
    }
}
