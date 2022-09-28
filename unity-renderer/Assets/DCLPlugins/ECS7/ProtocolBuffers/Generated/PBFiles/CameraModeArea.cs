// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: CameraModeArea.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
/// <summary>Holder for reflection information generated from CameraModeArea.proto</summary>
public static partial class CameraModeAreaReflection {

  #region Descriptor
  /// <summary>File descriptor for CameraModeArea.proto</summary>
  public static pbr::FileDescriptor Descriptor {
    get { return descriptor; }
  }
  private static pbr::FileDescriptor descriptor;

  static CameraModeAreaReflection() {
    byte[] descriptorData = global::System.Convert.FromBase64String(
        string.Concat(
          "ChRDYW1lcmFNb2RlQXJlYS5wcm90bxoUY29tbW9uL1ZlY3RvcjMucHJvdG8a",
          "HGNvbW1vbi9DYW1lcmFNb2RlVmFsdWUucHJvdG8iSgoQUEJDYW1lcmFNb2Rl",
          "QXJlYRIWCgRhcmVhGAEgASgLMgguVmVjdG9yMxIeCgRtb2RlGAIgASgOMhAu",
          "Q2FtZXJhTW9kZVZhbHVlYgZwcm90bzM="));
    descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
        new pbr::FileDescriptor[] { global::Vector3Reflection.Descriptor, global::CameraModeValueReflection.Descriptor, },
        new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
          new pbr::GeneratedClrTypeInfo(typeof(global::PBCameraModeArea), global::PBCameraModeArea.Parser, new[]{ "Area", "Mode" }, null, null, null, null)
        }));
  }
  #endregion

}
#region Messages
public sealed partial class PBCameraModeArea : pb::IMessage<PBCameraModeArea>
#if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    , pb::IBufferMessage
#endif
{
  private static readonly pb::MessageParser<PBCameraModeArea> _parser = new pb::MessageParser<PBCameraModeArea>(() => new PBCameraModeArea());
  private pb::UnknownFieldSet _unknownFields;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public static pb::MessageParser<PBCameraModeArea> Parser { get { return _parser; } }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public static pbr::MessageDescriptor Descriptor {
    get { return global::CameraModeAreaReflection.Descriptor.MessageTypes[0]; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  pbr::MessageDescriptor pb::IMessage.Descriptor {
    get { return Descriptor; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public PBCameraModeArea() {
    OnConstruction();
  }

  partial void OnConstruction();

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public PBCameraModeArea(PBCameraModeArea other) : this() {
    area_ = other.area_ != null ? other.area_.Clone() : null;
    mode_ = other.mode_;
    _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public PBCameraModeArea Clone() {
    return new PBCameraModeArea(this);
  }

  /// <summary>Field number for the "area" field.</summary>
  public const int AreaFieldNumber = 1;
  private global::Vector3 area_;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public global::Vector3 Area {
    get { return area_; }
    set {
      area_ = value;
    }
  }

  /// <summary>Field number for the "mode" field.</summary>
  public const int ModeFieldNumber = 2;
  private global::CameraModeValue mode_ = global::CameraModeValue.FirstPerson;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public global::CameraModeValue Mode {
    get { return mode_; }
    set {
      mode_ = value;
    }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public override bool Equals(object other) {
    return Equals(other as PBCameraModeArea);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public bool Equals(PBCameraModeArea other) {
    if (ReferenceEquals(other, null)) {
      return false;
    }
    if (ReferenceEquals(other, this)) {
      return true;
    }
    if (!object.Equals(Area, other.Area)) return false;
    if (Mode != other.Mode) return false;
    return Equals(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public override int GetHashCode() {
    int hash = 1;
    if (area_ != null) hash ^= Area.GetHashCode();
    if (Mode != global::CameraModeValue.FirstPerson) hash ^= Mode.GetHashCode();
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
    if (area_ != null) {
      output.WriteRawTag(10);
      output.WriteMessage(Area);
    }
    if (Mode != global::CameraModeValue.FirstPerson) {
      output.WriteRawTag(16);
      output.WriteEnum((int) Mode);
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
    if (area_ != null) {
      output.WriteRawTag(10);
      output.WriteMessage(Area);
    }
    if (Mode != global::CameraModeValue.FirstPerson) {
      output.WriteRawTag(16);
      output.WriteEnum((int) Mode);
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
    if (area_ != null) {
      size += 1 + pb::CodedOutputStream.ComputeMessageSize(Area);
    }
    if (Mode != global::CameraModeValue.FirstPerson) {
      size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Mode);
    }
    if (_unknownFields != null) {
      size += _unknownFields.CalculateSize();
    }
    return size;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public void MergeFrom(PBCameraModeArea other) {
    if (other == null) {
      return;
    }
    if (other.area_ != null) {
      if (area_ == null) {
        Area = new global::Vector3();
      }
      Area.MergeFrom(other.Area);
    }
    if (other.Mode != global::CameraModeValue.FirstPerson) {
      Mode = other.Mode;
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
          if (area_ == null) {
            Area = new global::Vector3();
          }
          input.ReadMessage(Area);
          break;
        }
        case 16: {
          Mode = (global::CameraModeValue) input.ReadEnum();
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
          if (area_ == null) {
            Area = new global::Vector3();
          }
          input.ReadMessage(Area);
          break;
        }
        case 16: {
          Mode = (global::CameraModeValue) input.ReadEnum();
          break;
        }
      }
    }
  }
  #endif

}

#endregion


#endregion Designer generated code
