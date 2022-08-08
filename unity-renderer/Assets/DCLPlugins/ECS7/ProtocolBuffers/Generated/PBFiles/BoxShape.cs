// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: BoxShape.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace DCL.ECSComponents {

  /// <summary>Holder for reflection information generated from BoxShape.proto</summary>
  public static partial class BoxShapeReflection {

    #region Descriptor
    /// <summary>File descriptor for BoxShape.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static BoxShapeReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cg5Cb3hTaGFwZS5wcm90bxIQZGVjZW50cmFsYW5kLmVjcyKlAQoKUEJCb3hT",
            "aGFwZRIcCg93aXRoX2NvbGxpc2lvbnMYASABKAhIAIgBARIfChJpc19wb2lu",
            "dGVyX2Jsb2NrZXIYAiABKAhIAYgBARIUCgd2aXNpYmxlGAMgASgISAKIAQES",
            "CwoDdXZzGAQgAygCQhIKEF93aXRoX2NvbGxpc2lvbnNCFQoTX2lzX3BvaW50",
            "ZXJfYmxvY2tlckIKCghfdmlzaWJsZUIUqgIRRENMLkVDU0NvbXBvbmVudHNi",
            "BnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::DCL.ECSComponents.PBBoxShape), global::DCL.ECSComponents.PBBoxShape.Parser, new[]{ "WithCollisions", "IsPointerBlocker", "Visible", "Uvs" }, new[]{ "WithCollisions", "IsPointerBlocker", "Visible" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class PBBoxShape : pb::IMessage<PBBoxShape>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<PBBoxShape> _parser = new pb::MessageParser<PBBoxShape>(() => new PBBoxShape());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<PBBoxShape> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::DCL.ECSComponents.BoxShapeReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBBoxShape() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBBoxShape(PBBoxShape other) : this() {
      _hasBits0 = other._hasBits0;
      withCollisions_ = other.withCollisions_;
      isPointerBlocker_ = other.isPointerBlocker_;
      visible_ = other.visible_;
      uvs_ = other.uvs_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBBoxShape Clone() {
      return new PBBoxShape(this);
    }

    /// <summary>Field number for the "with_collisions" field.</summary>
    public const int WithCollisionsFieldNumber = 1;
    private bool withCollisions_;
    /// <summary>
    /// default=true
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool WithCollisions {
      get { if ((_hasBits0 & 1) != 0) { return withCollisions_; } else { return false; } }
      set {
        _hasBits0 |= 1;
        withCollisions_ = value;
      }
    }
    /// <summary>Gets whether the "with_collisions" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasWithCollisions {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "with_collisions" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearWithCollisions() {
      _hasBits0 &= ~1;
    }

    /// <summary>Field number for the "is_pointer_blocker" field.</summary>
    public const int IsPointerBlockerFieldNumber = 2;
    private bool isPointerBlocker_;
    /// <summary>
    /// default=true
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool IsPointerBlocker {
      get { if ((_hasBits0 & 2) != 0) { return isPointerBlocker_; } else { return false; } }
      set {
        _hasBits0 |= 2;
        isPointerBlocker_ = value;
      }
    }
    /// <summary>Gets whether the "is_pointer_blocker" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasIsPointerBlocker {
      get { return (_hasBits0 & 2) != 0; }
    }
    /// <summary>Clears the value of the "is_pointer_blocker" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearIsPointerBlocker() {
      _hasBits0 &= ~2;
    }

    /// <summary>Field number for the "visible" field.</summary>
    public const int VisibleFieldNumber = 3;
    private bool visible_;
    /// <summary>
    /// default=true
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Visible {
      get { if ((_hasBits0 & 4) != 0) { return visible_; } else { return false; } }
      set {
        _hasBits0 |= 4;
        visible_ = value;
      }
    }
    /// <summary>Gets whether the "visible" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasVisible {
      get { return (_hasBits0 & 4) != 0; }
    }
    /// <summary>Clears the value of the "visible" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearVisible() {
      _hasBits0 &= ~4;
    }

    /// <summary>Field number for the "uvs" field.</summary>
    public const int UvsFieldNumber = 4;
    private static readonly pb::FieldCodec<float> _repeated_uvs_codec
        = pb::FieldCodec.ForFloat(34);
    private readonly pbc::RepeatedField<float> uvs_ = new pbc::RepeatedField<float>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<float> Uvs {
      get { return uvs_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as PBBoxShape);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(PBBoxShape other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (WithCollisions != other.WithCollisions) return false;
      if (IsPointerBlocker != other.IsPointerBlocker) return false;
      if (Visible != other.Visible) return false;
      if(!uvs_.Equals(other.uvs_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (HasWithCollisions) hash ^= WithCollisions.GetHashCode();
      if (HasIsPointerBlocker) hash ^= IsPointerBlocker.GetHashCode();
      if (HasVisible) hash ^= Visible.GetHashCode();
      hash ^= uvs_.GetHashCode();
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
      if (HasWithCollisions) {
        output.WriteRawTag(8);
        output.WriteBool(WithCollisions);
      }
      if (HasIsPointerBlocker) {
        output.WriteRawTag(16);
        output.WriteBool(IsPointerBlocker);
      }
      if (HasVisible) {
        output.WriteRawTag(24);
        output.WriteBool(Visible);
      }
      uvs_.WriteTo(output, _repeated_uvs_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (HasWithCollisions) {
        output.WriteRawTag(8);
        output.WriteBool(WithCollisions);
      }
      if (HasIsPointerBlocker) {
        output.WriteRawTag(16);
        output.WriteBool(IsPointerBlocker);
      }
      if (HasVisible) {
        output.WriteRawTag(24);
        output.WriteBool(Visible);
      }
      uvs_.WriteTo(ref output, _repeated_uvs_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (HasWithCollisions) {
        size += 1 + 1;
      }
      if (HasIsPointerBlocker) {
        size += 1 + 1;
      }
      if (HasVisible) {
        size += 1 + 1;
      }
      size += uvs_.CalculateSize(_repeated_uvs_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(PBBoxShape other) {
      if (other == null) {
        return;
      }
      if (other.HasWithCollisions) {
        WithCollisions = other.WithCollisions;
      }
      if (other.HasIsPointerBlocker) {
        IsPointerBlocker = other.IsPointerBlocker;
      }
      if (other.HasVisible) {
        Visible = other.Visible;
      }
      uvs_.Add(other.uvs_);
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
            WithCollisions = input.ReadBool();
            break;
          }
          case 16: {
            IsPointerBlocker = input.ReadBool();
            break;
          }
          case 24: {
            Visible = input.ReadBool();
            break;
          }
          case 34:
          case 37: {
            uvs_.AddEntriesFrom(input, _repeated_uvs_codec);
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
            WithCollisions = input.ReadBool();
            break;
          }
          case 16: {
            IsPointerBlocker = input.ReadBool();
            break;
          }
          case 24: {
            Visible = input.ReadBool();
            break;
          }
          case 34:
          case 37: {
            uvs_.AddEntriesFrom(ref input, _repeated_uvs_codec);
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
