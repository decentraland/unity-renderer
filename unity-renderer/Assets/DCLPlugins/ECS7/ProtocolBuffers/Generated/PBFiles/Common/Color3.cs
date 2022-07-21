// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: Color3.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
/// <summary>Holder for reflection information generated from Color3.proto</summary>
public static partial class Color3Reflection {

  #region Descriptor
  /// <summary>File descriptor for Color3.proto</summary>
  public static pbr::FileDescriptor Descriptor {
    get { return descriptor; }
  }
  private static pbr::FileDescriptor descriptor;

  static Color3Reflection() {
    byte[] descriptorData = global::System.Convert.FromBase64String(
        string.Concat(
          "CgxDb2xvcjMucHJvdG8iKQoGQ29sb3IzEgkKAXIYASABKAISCQoBZxgCIAEo",
          "AhIJCgFiGAMgASgCYgZwcm90bzM="));
    descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
        new pbr::FileDescriptor[] { },
        new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
          new pbr::GeneratedClrTypeInfo(typeof(global::Color3), global::Color3.Parser, new[]{ "R", "G", "B" }, null, null, null, null)
        }));
  }
  #endregion

}
#region Messages
public sealed partial class Color3 : pb::IMessage<Color3> {
  private static readonly pb::MessageParser<Color3> _parser = new pb::MessageParser<Color3>(() => new Color3());
  private pb::UnknownFieldSet _unknownFields;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public static pb::MessageParser<Color3> Parser { get { return _parser; } }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public static pbr::MessageDescriptor Descriptor {
    get { return global::Color3Reflection.Descriptor.MessageTypes[0]; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  pbr::MessageDescriptor pb::IMessage.Descriptor {
    get { return Descriptor; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public Color3() {
    OnConstruction();
  }

  partial void OnConstruction();

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public Color3(Color3 other) : this() {
    r_ = other.r_;
    g_ = other.g_;
    b_ = other.b_;
    _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public Color3 Clone() {
    return new Color3(this);
  }

  /// <summary>Field number for the "r" field.</summary>
  public const int RFieldNumber = 1;
  private float r_;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public float R {
    get { return r_; }
    set {
      r_ = value;
    }
  }

  /// <summary>Field number for the "g" field.</summary>
  public const int GFieldNumber = 2;
  private float g_;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public float G {
    get { return g_; }
    set {
      g_ = value;
    }
  }

  /// <summary>Field number for the "b" field.</summary>
  public const int BFieldNumber = 3;
  private float b_;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public float B {
    get { return b_; }
    set {
      b_ = value;
    }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override bool Equals(object other) {
    return Equals(other as Color3);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public bool Equals(Color3 other) {
    if (ReferenceEquals(other, null)) {
      return false;
    }
    if (ReferenceEquals(other, this)) {
      return true;
    }
    if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(R, other.R)) return false;
    if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(G, other.G)) return false;
    if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(B, other.B)) return false;
    return Equals(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override int GetHashCode() {
    int hash = 1;
    if (R != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(R);
    if (G != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(G);
    if (B != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(B);
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
    if (R != 0F) {
      output.WriteRawTag(13);
      output.WriteFloat(R);
    }
    if (G != 0F) {
      output.WriteRawTag(21);
      output.WriteFloat(G);
    }
    if (B != 0F) {
      output.WriteRawTag(29);
      output.WriteFloat(B);
    }
    if (_unknownFields != null) {
      _unknownFields.WriteTo(output);
    }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public int CalculateSize() {
    int size = 0;
    if (R != 0F) {
      size += 1 + 4;
    }
    if (G != 0F) {
      size += 1 + 4;
    }
    if (B != 0F) {
      size += 1 + 4;
    }
    if (_unknownFields != null) {
      size += _unknownFields.CalculateSize();
    }
    return size;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void MergeFrom(Color3 other) {
    if (other == null) {
      return;
    }
    if (other.R != 0F) {
      R = other.R;
    }
    if (other.G != 0F) {
      G = other.G;
    }
    if (other.B != 0F) {
      B = other.B;
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
        case 13: {
          R = input.ReadFloat();
          break;
        }
        case 21: {
          G = input.ReadFloat();
          break;
        }
        case 29: {
          B = input.ReadFloat();
          break;
        }
      }
    }
  }

}

#endregion


#endregion Designer generated code
