using System;

namespace DCL.Emotes
{
    public class EmoteBodyId
    {
        public string BodyShapeId { get; }
        public string EmoteId { get; }

        public EmoteBodyId(string bodyShapeId, string emoteId)
        {
            this.BodyShapeId = bodyShapeId;
            this.EmoteId = emoteId;
        }

        public override string ToString() =>
            $"{BodyShapeId}:{EmoteId}";

        private bool Equals(EmoteBodyId other) =>
            BodyShapeId == other.BodyShapeId && EmoteId == other.EmoteId;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((EmoteBodyId)obj);
        }

        public override int GetHashCode() =>
            HashCode.Combine(BodyShapeId, EmoteId);
    }
}
