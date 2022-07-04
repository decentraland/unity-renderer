// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: OnPointerDownResult.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace DCL.ECSComponents {

  /// <summary>Holder for reflection information generated from OnPointerDownResult.proto</summary>
  public static partial class OnPointerDownResultReflection {

    #region Descriptor
    /// <summary>File descriptor for OnPointerDownResult.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static OnPointerDownResultReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChlPblBvaW50ZXJEb3duUmVzdWx0LnByb3RvEhBkZWNlbnRyYWxhbmQuZWNz",
            "GhRjb21tb24vVmVjdG9yMy5wcm90byLIAQoVUEJPblBvaW50ZXJEb3duUmVz",
            "dWx0Eg4KBmJ1dHRvbhgCIAEoBRIQCghtZXNoTmFtZRgDIAEoCRIYCgZvcmln",
            "aW4YBCABKAsyCC5WZWN0b3IzEhsKCWRpcmVjdGlvbhgFIAEoCzIILlZlY3Rv",
            "cjMSFwoFcG9pbnQYBiABKAsyCC5WZWN0b3IzEhgKBm5vcm1hbBgHIAEoCzII",
            "LlZlY3RvcjMSEAoIZGlzdGFuY2UYCCABKAISEQoJdGltZXN0YW1wGAkgASgF",
            "QhSqAhFEQ0wuRUNTQ29tcG9uZW50c2IGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Vector3Reflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::DCL.ECSComponents.PBOnPointerDownResult), global::DCL.ECSComponents.PBOnPointerDownResult.Parser, new[]{ "Button", "MeshName", "Origin", "Direction", "Point", "Normal", "Distance", "Timestamp" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class PBOnPointerDownResult : pb::IMessage<PBOnPointerDownResult> {
    private static readonly pb::MessageParser<PBOnPointerDownResult> _parser = new pb::MessageParser<PBOnPointerDownResult>(() => new PBOnPointerDownResult());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<PBOnPointerDownResult> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::DCL.ECSComponents.OnPointerDownResultReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public PBOnPointerDownResult() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public PBOnPointerDownResult(PBOnPointerDownResult other) : this() {
      button_ = other.button_;
      meshName_ = other.meshName_;
      origin_ = other.origin_ != null ? other.origin_.Clone() : null;
      direction_ = other.direction_ != null ? other.direction_.Clone() : null;
      point_ = other.point_ != null ? other.point_.Clone() : null;
      normal_ = other.normal_ != null ? other.normal_.Clone() : null;
      distance_ = other.distance_;
      timestamp_ = other.timestamp_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public PBOnPointerDownResult Clone() {
      return new PBOnPointerDownResult(this);
    }

    /// <summary>Field number for the "button" field.</summary>
    public const int ButtonFieldNumber = 2;
    private int button_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Button {
      get { return button_; }
      set {
        button_ = value;
      }
    }

    /// <summary>Field number for the "meshName" field.</summary>
    public const int MeshNameFieldNumber = 3;
    private string meshName_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string MeshName {
      get { return meshName_; }
      set {
        meshName_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "origin" field.</summary>
    public const int OriginFieldNumber = 4;
    private global::Vector3 origin_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Vector3 Origin {
      get { return origin_; }
      set {
        origin_ = value;
      }
    }

    /// <summary>Field number for the "direction" field.</summary>
    public const int DirectionFieldNumber = 5;
    private global::Vector3 direction_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Vector3 Direction {
      get { return direction_; }
      set {
        direction_ = value;
      }
    }

    /// <summary>Field number for the "point" field.</summary>
    public const int PointFieldNumber = 6;
    private global::Vector3 point_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Vector3 Point {
      get { return point_; }
      set {
        point_ = value;
      }
    }

    /// <summary>Field number for the "normal" field.</summary>
    public const int NormalFieldNumber = 7;
    private global::Vector3 normal_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Vector3 Normal {
      get { return normal_; }
      set {
        normal_ = value;
      }
    }

    /// <summary>Field number for the "distance" field.</summary>
    public const int DistanceFieldNumber = 8;
    private float distance_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float Distance {
      get { return distance_; }
      set {
        distance_ = value;
      }
    }

    /// <summary>Field number for the "timestamp" field.</summary>
    public const int TimestampFieldNumber = 9;
    private int timestamp_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Timestamp {
      get { return timestamp_; }
      set {
        timestamp_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as PBOnPointerDownResult);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(PBOnPointerDownResult other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Button != other.Button) return false;
      if (MeshName != other.MeshName) return false;
      if (!object.Equals(Origin, other.Origin)) return false;
      if (!object.Equals(Direction, other.Direction)) return false;
      if (!object.Equals(Point, other.Point)) return false;
      if (!object.Equals(Normal, other.Normal)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Distance, other.Distance)) return false;
      if (Timestamp != other.Timestamp) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Button != 0) hash ^= Button.GetHashCode();
      if (MeshName.Length != 0) hash ^= MeshName.GetHashCode();
      if (origin_ != null) hash ^= Origin.GetHashCode();
      if (direction_ != null) hash ^= Direction.GetHashCode();
      if (point_ != null) hash ^= Point.GetHashCode();
      if (normal_ != null) hash ^= Normal.GetHashCode();
      if (Distance != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Distance);
      if (Timestamp != 0) hash ^= Timestamp.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Button != 0) {
        output.WriteRawTag(16);
        output.WriteInt32(Button);
      }
      if (MeshName.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(MeshName);
      }
      if (origin_ != null) {
        output.WriteRawTag(34);
        output.WriteMessage(Origin);
      }
      if (direction_ != null) {
        output.WriteRawTag(42);
        output.WriteMessage(Direction);
      }
      if (point_ != null) {
        output.WriteRawTag(50);
        output.WriteMessage(Point);
      }
      if (normal_ != null) {
        output.WriteRawTag(58);
        output.WriteMessage(Normal);
      }
      if (Distance != 0F) {
        output.WriteRawTag(69);
        output.WriteFloat(Distance);
      }
      if (Timestamp != 0) {
        output.WriteRawTag(72);
        output.WriteInt32(Timestamp);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Button != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Button);
      }
      if (MeshName.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(MeshName);
      }
      if (origin_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Origin);
      }
      if (direction_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Direction);
      }
      if (point_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Point);
      }
      if (normal_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Normal);
      }
      if (Distance != 0F) {
        size += 1 + 4;
      }
      if (Timestamp != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Timestamp);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(PBOnPointerDownResult other) {
      if (other == null) {
        return;
      }
      if (other.Button != 0) {
        Button = other.Button;
      }
      if (other.MeshName.Length != 0) {
        MeshName = other.MeshName;
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
      if (other.point_ != null) {
        if (point_ == null) {
          Point = new global::Vector3();
        }
        Point.MergeFrom(other.Point);
      }
      if (other.normal_ != null) {
        if (normal_ == null) {
          Normal = new global::Vector3();
        }
        Normal.MergeFrom(other.Normal);
      }
      if (other.Distance != 0F) {
        Distance = other.Distance;
      }
      if (other.Timestamp != 0) {
        Timestamp = other.Timestamp;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 16: {
            Button = input.ReadInt32();
            break;
          }
          case 26: {
            MeshName = input.ReadString();
            break;
          }
          case 34: {
            if (origin_ == null) {
              Origin = new global::Vector3();
            }
            input.ReadMessage(Origin);
            break;
          }
          case 42: {
            if (direction_ == null) {
              Direction = new global::Vector3();
            }
            input.ReadMessage(Direction);
            break;
          }
          case 50: {
            if (point_ == null) {
              Point = new global::Vector3();
            }
            input.ReadMessage(Point);
            break;
          }
          case 58: {
            if (normal_ == null) {
              Normal = new global::Vector3();
            }
            input.ReadMessage(Normal);
            break;
          }
          case 69: {
            Distance = input.ReadFloat();
            break;
          }
          case 72: {
            Timestamp = input.ReadInt32();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
