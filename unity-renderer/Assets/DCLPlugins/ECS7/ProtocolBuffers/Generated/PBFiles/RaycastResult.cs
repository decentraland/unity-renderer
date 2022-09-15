// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: RaycastResult.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace DCL.ECSComponents {

  /// <summary>Holder for reflection information generated from RaycastResult.proto</summary>
  public static partial class RaycastResultReflection {

    #region Descriptor
    /// <summary>File descriptor for RaycastResult.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static RaycastResultReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChNSYXljYXN0UmVzdWx0LnByb3RvEhBkZWNlbnRyYWxhbmQuZWNzGhRjb21t",
            "b24vVmVjdG9yMy5wcm90byKHAQoPUEJSYXljYXN0UmVzdWx0EhEKCXRpbWVz",
            "dGFtcBgBIAEoBRIYCgZvcmlnaW4YAiABKAsyCC5WZWN0b3IzEhsKCWRpcmVj",
            "dGlvbhgDIAEoCzIILlZlY3RvcjMSKgoEaGl0cxgEIAMoCzIcLmRlY2VudHJh",
            "bGFuZC5lY3MuUmF5Y2FzdEhpdCLZAQoKUmF5Y2FzdEhpdBIaCghwb3NpdGlv",
            "bhgBIAEoCzIILlZlY3RvcjMSGAoGb3JpZ2luGAIgASgLMgguVmVjdG9yMxIb",
            "CglkaXJlY3Rpb24YAyABKAsyCC5WZWN0b3IzEhwKCm5vcm1hbF9oaXQYBCAB",
            "KAsyCC5WZWN0b3IzEg4KBmxlbmd0aBgFIAEoAhIWCgltZXNoX25hbWUYBiAB",
            "KAlIAIgBARIWCgllbnRpdHlfaWQYByABKANIAYgBAUIMCgpfbWVzaF9uYW1l",
            "QgwKCl9lbnRpdHlfaWRCFKoCEURDTC5FQ1NDb21wb25lbnRzYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Vector3Reflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::DCL.ECSComponents.PBRaycastResult), global::DCL.ECSComponents.PBRaycastResult.Parser, new[]{ "Timestamp", "Origin", "Direction", "Hits" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::DCL.ECSComponents.RaycastHit), global::DCL.ECSComponents.RaycastHit.Parser, new[]{ "Position", "Origin", "Direction", "NormalHit", "Length", "MeshName", "EntityId" }, new[]{ "MeshName", "EntityId" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class PBRaycastResult : pb::IMessage<PBRaycastResult>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<PBRaycastResult> _parser = new pb::MessageParser<PBRaycastResult>(() => new PBRaycastResult());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<PBRaycastResult> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::DCL.ECSComponents.RaycastResultReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBRaycastResult() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBRaycastResult(PBRaycastResult other) : this() {
      timestamp_ = other.timestamp_;
      origin_ = other.origin_ != null ? other.origin_.Clone() : null;
      direction_ = other.direction_ != null ? other.direction_.Clone() : null;
      hits_ = other.hits_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBRaycastResult Clone() {
      return new PBRaycastResult(this);
    }

    /// <summary>Field number for the "timestamp" field.</summary>
    public const int TimestampFieldNumber = 1;
    private int timestamp_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int Timestamp {
      get { return timestamp_; }
      set {
        timestamp_ = value;
      }
    }

    /// <summary>Field number for the "origin" field.</summary>
    public const int OriginFieldNumber = 2;
    private global::Vector3 origin_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Vector3 Origin {
      get { return origin_; }
      set {
        origin_ = value;
      }
    }

    /// <summary>Field number for the "direction" field.</summary>
    public const int DirectionFieldNumber = 3;
    private global::Vector3 direction_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Vector3 Direction {
      get { return direction_; }
      set {
        direction_ = value;
      }
    }

    /// <summary>Field number for the "hits" field.</summary>
    public const int HitsFieldNumber = 4;
    private static readonly pb::FieldCodec<global::DCL.ECSComponents.RaycastHit> _repeated_hits_codec
        = pb::FieldCodec.ForMessage(34, global::DCL.ECSComponents.RaycastHit.Parser);
    private readonly pbc::RepeatedField<global::DCL.ECSComponents.RaycastHit> hits_ = new pbc::RepeatedField<global::DCL.ECSComponents.RaycastHit>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<global::DCL.ECSComponents.RaycastHit> Hits {
      get { return hits_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as PBRaycastResult);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(PBRaycastResult other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Timestamp != other.Timestamp) return false;
      if (!object.Equals(Origin, other.Origin)) return false;
      if (!object.Equals(Direction, other.Direction)) return false;
      if(!hits_.Equals(other.hits_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (Timestamp != 0) hash ^= Timestamp.GetHashCode();
      if (origin_ != null) hash ^= Origin.GetHashCode();
      if (direction_ != null) hash ^= Direction.GetHashCode();
      hash ^= hits_.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (Timestamp != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(Timestamp);
      }
      if (origin_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(Origin);
      }
      if (direction_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(Direction);
      }
      hits_.WriteTo(output, _repeated_hits_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (Timestamp != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(Timestamp);
      }
      if (origin_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(Origin);
      }
      if (direction_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(Direction);
      }
      hits_.WriteTo(ref output, _repeated_hits_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (Timestamp != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Timestamp);
      }
      if (origin_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Origin);
      }
      if (direction_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Direction);
      }
      size += hits_.CalculateSize(_repeated_hits_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(PBRaycastResult other) {
      if (other == null) {
        return;
      }
      if (other.Timestamp != 0) {
        Timestamp = other.Timestamp;
      }
      if (other.origin_ != null) {
        if (origin_ == null) {
          Origin = new global::Vector3();
        }
        Origin.MergeFrom(other.Origin);
      }
      if (other.direction_ != null) {
        if (direction_ == null) {
          Direction = new global::Vector3();
        }
        Direction.MergeFrom(other.Direction);
      }
      hits_.Add(other.hits_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            Timestamp = input.ReadInt32();
            break;
          }
          case 18: {
            if (origin_ == null) {
              Origin = new global::Vector3();
            }
            input.ReadMessage(Origin);
            break;
          }
          case 26: {
            if (direction_ == null) {
              Direction = new global::Vector3();
            }
            input.ReadMessage(Direction);
            break;
          }
          case 34: {
            hits_.AddEntriesFrom(input, _repeated_hits_codec);
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 8: {
            Timestamp = input.ReadInt32();
            break;
          }
          case 18: {
            if (origin_ == null) {
              Origin = new global::Vector3();
            }
            input.ReadMessage(Origin);
            break;
          }
          case 26: {
            if (direction_ == null) {
              Direction = new global::Vector3();
            }
            input.ReadMessage(Direction);
            break;
          }
          case 34: {
            hits_.AddEntriesFrom(ref input, _repeated_hits_codec);
            break;
          }
        }
      }
    }
    #endif

  }

  /// <summary>
  /// Position will be relative to the scene  
  /// </summary>
  public sealed partial class RaycastHit : pb::IMessage<RaycastHit>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<RaycastHit> _parser = new pb::MessageParser<RaycastHit>(() => new RaycastHit());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<RaycastHit> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::DCL.ECSComponents.RaycastResultReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public RaycastHit() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public RaycastHit(RaycastHit other) : this() {
      _hasBits0 = other._hasBits0;
      position_ = other.position_ != null ? other.position_.Clone() : null;
      origin_ = other.origin_ != null ? other.origin_.Clone() : null;
      direction_ = other.direction_ != null ? other.direction_.Clone() : null;
      normalHit_ = other.normalHit_ != null ? other.normalHit_.Clone() : null;
      length_ = other.length_;
      meshName_ = other.meshName_;
      entityId_ = other.entityId_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public RaycastHit Clone() {
      return new RaycastHit(this);
    }

    /// <summary>Field number for the "position" field.</summary>
    public const int PositionFieldNumber = 1;
    private global::Vector3 position_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Vector3 Position {
      get { return position_; }
      set {
        position_ = value;
      }
    }

    /// <summary>Field number for the "origin" field.</summary>
    public const int OriginFieldNumber = 2;
    private global::Vector3 origin_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Vector3 Origin {
      get { return origin_; }
      set {
        origin_ = value;
      }
    }

    /// <summary>Field number for the "direction" field.</summary>
    public const int DirectionFieldNumber = 3;
    private global::Vector3 direction_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Vector3 Direction {
      get { return direction_; }
      set {
        direction_ = value;
      }
    }

    /// <summary>Field number for the "normal_hit" field.</summary>
    public const int NormalHitFieldNumber = 4;
    private global::Vector3 normalHit_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Vector3 NormalHit {
      get { return normalHit_; }
      set {
        normalHit_ = value;
      }
    }

    /// <summary>Field number for the "length" field.</summary>
    public const int LengthFieldNumber = 5;
    private float length_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Length {
      get { return length_; }
      set {
        length_ = value;
      }
    }

    /// <summary>Field number for the "mesh_name" field.</summary>
    public const int MeshNameFieldNumber = 6;
    private string meshName_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string MeshName {
      get { return meshName_ ?? ""; }
      set {
        meshName_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }
    /// <summary>Gets whether the "mesh_name" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasMeshName {
      get { return meshName_ != null; }
    }
    /// <summary>Clears the value of the "mesh_name" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearMeshName() {
      meshName_ = null;
    }

    /// <summary>Field number for the "entity_id" field.</summary>
    public const int EntityIdFieldNumber = 7;
    private long entityId_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public long EntityId {
      get { if ((_hasBits0 & 1) != 0) { return entityId_; } else { return 0L; } }
      set {
        _hasBits0 |= 1;
        entityId_ = value;
      }
    }
    /// <summary>Gets whether the "entity_id" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasEntityId {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "entity_id" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearEntityId() {
      _hasBits0 &= ~1;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as RaycastHit);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(RaycastHit other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Position, other.Position)) return false;
      if (!object.Equals(Origin, other.Origin)) return false;
      if (!object.Equals(Direction, other.Direction)) return false;
      if (!object.Equals(NormalHit, other.NormalHit)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Length, other.Length)) return false;
      if (MeshName != other.MeshName) return false;
      if (EntityId != other.EntityId) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (position_ != null) hash ^= Position.GetHashCode();
      if (origin_ != null) hash ^= Origin.GetHashCode();
      if (direction_ != null) hash ^= Direction.GetHashCode();
      if (normalHit_ != null) hash ^= NormalHit.GetHashCode();
      if (Length != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Length);
      if (HasMeshName) hash ^= MeshName.GetHashCode();
      if (HasEntityId) hash ^= EntityId.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (position_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Position);
      }
      if (origin_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(Origin);
      }
      if (direction_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(Direction);
      }
      if (normalHit_ != null) {
        output.WriteRawTag(34);
        output.WriteMessage(NormalHit);
      }
      if (Length != 0F) {
        output.WriteRawTag(45);
        output.WriteFloat(Length);
      }
      if (HasMeshName) {
        output.WriteRawTag(50);
        output.WriteString(MeshName);
      }
      if (HasEntityId) {
        output.WriteRawTag(56);
        output.WriteInt64(EntityId);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (position_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Position);
      }
      if (origin_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(Origin);
      }
      if (direction_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(Direction);
      }
      if (normalHit_ != null) {
        output.WriteRawTag(34);
        output.WriteMessage(NormalHit);
      }
      if (Length != 0F) {
        output.WriteRawTag(45);
        output.WriteFloat(Length);
      }
      if (HasMeshName) {
        output.WriteRawTag(50);
        output.WriteString(MeshName);
      }
      if (HasEntityId) {
        output.WriteRawTag(56);
        output.WriteInt64(EntityId);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (position_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Position);
      }
      if (origin_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Origin);
      }
      if (direction_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Direction);
      }
      if (normalHit_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(NormalHit);
      }
      if (Length != 0F) {
        size += 1 + 4;
      }
      if (HasMeshName) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(MeshName);
      }
      if (HasEntityId) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(EntityId);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(RaycastHit other) {
      if (other == null) {
        return;
      }
      if (other.position_ != null) {
        if (position_ == null) {
          Position = new global::Vector3();
        }
        Position.MergeFrom(other.Position);
      }
      if (other.origin_ != null) {
        if (origin_ == null) {
          Origin = new global::Vector3();
        }
        Origin.MergeFrom(other.Origin);
      }
      if (other.direction_ != null) {
        if (direction_ == null) {
          Direction = new global::Vector3();
        }
        Direction.MergeFrom(other.Direction);
      }
      if (other.normalHit_ != null) {
        if (normalHit_ == null) {
          NormalHit = new global::Vector3();
        }
        NormalHit.MergeFrom(other.NormalHit);
      }
      if (other.Length != 0F) {
        Length = other.Length;
      }
      if (other.HasMeshName) {
        MeshName = other.MeshName;
      }
      if (other.HasEntityId) {
        EntityId = other.EntityId;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            if (position_ == null) {
              Position = new global::Vector3();
            }
            input.ReadMessage(Position);
            break;
          }
          case 18: {
            if (origin_ == null) {
              Origin = new global::Vector3();
            }
            input.ReadMessage(Origin);
            break;
          }
          case 26: {
            if (direction_ == null) {
              Direction = new global::Vector3();
            }
            input.ReadMessage(Direction);
            break;
          }
          case 34: {
            if (normalHit_ == null) {
              NormalHit = new global::Vector3();
            }
            input.ReadMessage(NormalHit);
            break;
          }
          case 45: {
            Length = input.ReadFloat();
            break;
          }
          case 50: {
            MeshName = input.ReadString();
            break;
          }
          case 56: {
            EntityId = input.ReadInt64();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            if (position_ == null) {
              Position = new global::Vector3();
            }
            input.ReadMessage(Position);
            break;
          }
          case 18: {
            if (origin_ == null) {
              Origin = new global::Vector3();
            }
            input.ReadMessage(Origin);
            break;
          }
          case 26: {
            if (direction_ == null) {
              Direction = new global::Vector3();
            }
            input.ReadMessage(Direction);
            break;
          }
          case 34: {
            if (normalHit_ == null) {
              NormalHit = new global::Vector3();
            }
            input.ReadMessage(NormalHit);
            break;
          }
          case 45: {
            Length = input.ReadFloat();
            break;
          }
          case 50: {
            MeshName = input.ReadString();
            break;
          }
          case 56: {
            EntityId = input.ReadInt64();
            break;
          }
        }
      }
    }
    #endif

  }

  #endregion

}

#endregion Designer generated code
