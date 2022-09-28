// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: AvatarShape.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace DCL.ECSComponents {

  /// <summary>Holder for reflection information generated from AvatarShape.proto</summary>
  public static partial class AvatarShapeReflection {

    #region Descriptor
    /// <summary>File descriptor for AvatarShape.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static AvatarShapeReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChFBdmF0YXJTaGFwZS5wcm90bxIQZGVjZW50cmFsYW5kLmVjcxoTY29tbW9u",
            "L0NvbG9yMy5wcm90byLiAwoNUEJBdmF0YXJTaGFwZRIKCgJpZBgBIAEoCRIR",
            "CgRuYW1lGAIgASgJSACIAQESFwoKYm9keV9zaGFwZRgDIAEoCUgBiAEBEjEK",
            "CnNraW5fY29sb3IYBCABKAsyGC5kZWNlbnRyYWxhbmQuZWNzLkNvbG9yM0gC",
            "iAEBEjEKCmhhaXJfY29sb3IYBSABKAsyGC5kZWNlbnRyYWxhbmQuZWNzLkNv",
            "bG9yM0gDiAEBEjAKCWV5ZV9jb2xvchgGIAEoCzIYLmRlY2VudHJhbGFuZC5l",
            "Y3MuQ29sb3IzSASIAQESIgoVZXhwcmVzc2lvbl90cmlnZ2VyX2lkGAcgASgJ",
            "SAWIAQESKQocZXhwcmVzc2lvbl90cmlnZ2VyX3RpbWVzdGFtcBgIIAEoA0gG",
            "iAEBEhQKB3RhbGtpbmcYCSABKAhIB4gBARIRCgl3ZWFyYWJsZXMYCiADKAlC",
            "BwoFX25hbWVCDQoLX2JvZHlfc2hhcGVCDQoLX3NraW5fY29sb3JCDQoLX2hh",
            "aXJfY29sb3JCDAoKX2V5ZV9jb2xvckIYChZfZXhwcmVzc2lvbl90cmlnZ2Vy",
            "X2lkQh8KHV9leHByZXNzaW9uX3RyaWdnZXJfdGltZXN0YW1wQgoKCF90YWxr",
            "aW5nQhSqAhFEQ0wuRUNTQ29tcG9uZW50c2IGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::DCL.ECSComponents.Color3Reflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::DCL.ECSComponents.PBAvatarShape), global::DCL.ECSComponents.PBAvatarShape.Parser, new[]{ "Id", "Name", "BodyShape", "SkinColor", "HairColor", "EyeColor", "ExpressionTriggerId", "ExpressionTriggerTimestamp", "Talking", "Wearables" }, new[]{ "Name", "BodyShape", "SkinColor", "HairColor", "EyeColor", "ExpressionTriggerId", "ExpressionTriggerTimestamp", "Talking" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class PBAvatarShape : pb::IMessage<PBAvatarShape>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<PBAvatarShape> _parser = new pb::MessageParser<PBAvatarShape>(() => new PBAvatarShape());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<PBAvatarShape> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::DCL.ECSComponents.AvatarShapeReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBAvatarShape() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBAvatarShape(PBAvatarShape other) : this() {
      _hasBits0 = other._hasBits0;
      id_ = other.id_;
      name_ = other.name_;
      bodyShape_ = other.bodyShape_;
      skinColor_ = other.skinColor_ != null ? other.skinColor_.Clone() : null;
      hairColor_ = other.hairColor_ != null ? other.hairColor_.Clone() : null;
      eyeColor_ = other.eyeColor_ != null ? other.eyeColor_.Clone() : null;
      expressionTriggerId_ = other.expressionTriggerId_;
      expressionTriggerTimestamp_ = other.expressionTriggerTimestamp_;
      talking_ = other.talking_;
      wearables_ = other.wearables_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBAvatarShape Clone() {
      return new PBAvatarShape(this);
    }

    /// <summary>Field number for the "id" field.</summary>
    public const int IdFieldNumber = 1;
    private string id_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string Id {
      get { return id_; }
      set {
        id_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "name" field.</summary>
    public const int NameFieldNumber = 2;
    private string name_;
    /// <summary>
    /// default = NPC
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string Name {
      get { return name_ ?? ""; }
      set {
        name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }
    /// <summary>Gets whether the "name" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasName {
      get { return name_ != null; }
    }
    /// <summary>Clears the value of the "name" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearName() {
      name_ = null;
    }

    /// <summary>Field number for the "body_shape" field.</summary>
    public const int BodyShapeFieldNumber = 3;
    private string bodyShape_;
    /// <summary>
    /// default = urn:decentraland:off-chain:base-avatars:BaseFemale
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string BodyShape {
      get { return bodyShape_ ?? ""; }
      set {
        bodyShape_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }
    /// <summary>Gets whether the "body_shape" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasBodyShape {
      get { return bodyShape_ != null; }
    }
    /// <summary>Clears the value of the "body_shape" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearBodyShape() {
      bodyShape_ = null;
    }

    /// <summary>Field number for the "skin_color" field.</summary>
    public const int SkinColorFieldNumber = 4;
    private global::DCL.ECSComponents.Color3 skinColor_;
    /// <summary>
    /// default = Color3(R = 0.6f, G = 0.462f, B = 0.356f)
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::DCL.ECSComponents.Color3 SkinColor {
      get { return skinColor_; }
      set {
        skinColor_ = value;
      }
    }

    /// <summary>Field number for the "hair_color" field.</summary>
    public const int HairColorFieldNumber = 5;
    private global::DCL.ECSComponents.Color3 hairColor_;
    /// <summary>
    /// default = Color3(R = 0.283f, G = 0.142f, B = 0f)
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::DCL.ECSComponents.Color3 HairColor {
      get { return hairColor_; }
      set {
        hairColor_ = value;
      }
    }

    /// <summary>Field number for the "eye_color" field.</summary>
    public const int EyeColorFieldNumber = 6;
    private global::DCL.ECSComponents.Color3 eyeColor_;
    /// <summary>
    /// default = Color3(R = 0.6f, G = 0.462f, B = 0.356f)
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::DCL.ECSComponents.Color3 EyeColor {
      get { return eyeColor_; }
      set {
        eyeColor_ = value;
      }
    }

    /// <summary>Field number for the "expression_trigger_id" field.</summary>
    public const int ExpressionTriggerIdFieldNumber = 7;
    private string expressionTriggerId_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string ExpressionTriggerId {
      get { return expressionTriggerId_ ?? ""; }
      set {
        expressionTriggerId_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }
    /// <summary>Gets whether the "expression_trigger_id" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasExpressionTriggerId {
      get { return expressionTriggerId_ != null; }
    }
    /// <summary>Clears the value of the "expression_trigger_id" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearExpressionTriggerId() {
      expressionTriggerId_ = null;
    }

    /// <summary>Field number for the "expression_trigger_timestamp" field.</summary>
    public const int ExpressionTriggerTimestampFieldNumber = 8;
    private long expressionTriggerTimestamp_;
    /// <summary>
    /// default = timestamp
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public long ExpressionTriggerTimestamp {
      get { if ((_hasBits0 & 1) != 0) { return expressionTriggerTimestamp_; } else { return 0L; } }
      set {
        _hasBits0 |= 1;
        expressionTriggerTimestamp_ = value;
      }
    }
    /// <summary>Gets whether the "expression_trigger_timestamp" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasExpressionTriggerTimestamp {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "expression_trigger_timestamp" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearExpressionTriggerTimestamp() {
      _hasBits0 &= ~1;
    }

    /// <summary>Field number for the "talking" field.</summary>
    public const int TalkingFieldNumber = 9;
    private bool talking_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Talking {
      get { if ((_hasBits0 & 2) != 0) { return talking_; } else { return false; } }
      set {
        _hasBits0 |= 2;
        talking_ = value;
      }
    }
    /// <summary>Gets whether the "talking" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasTalking {
      get { return (_hasBits0 & 2) != 0; }
    }
    /// <summary>Clears the value of the "talking" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearTalking() {
      _hasBits0 &= ~2;
    }

    /// <summary>Field number for the "wearables" field.</summary>
    public const int WearablesFieldNumber = 10;
    private static readonly pb::FieldCodec<string> _repeated_wearables_codec
        = pb::FieldCodec.ForString(82);
    private readonly pbc::RepeatedField<string> wearables_ = new pbc::RepeatedField<string>();
    /// <summary>
    ///*
    /// default = ["urn:decentraland:off-chain:base-avatars:f_eyes_00", 
    ///  "urn:decentraland:off-chain:base-avatars:f_eyebrows_00",
    ///  "urn:decentraland:off-chain:base-avatars:f_mouth_00" 
    ///  "urn:decentraland:off-chain:base-avatars:standard_hair", 
    ///  "urn:decentraland:off-chain:base-avatars:f_simple_yellow_tshirt", 
    ///  "urn:decentraland:off-chain:base-avatars:f_brown_trousers", 
    ///  "urn:decentraland:off-chain:base-avatars:bun_shoes"]
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<string> Wearables {
      get { return wearables_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as PBAvatarShape);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(PBAvatarShape other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Id != other.Id) return false;
      if (Name != other.Name) return false;
      if (BodyShape != other.BodyShape) return false;
      if (!object.Equals(SkinColor, other.SkinColor)) return false;
      if (!object.Equals(HairColor, other.HairColor)) return false;
      if (!object.Equals(EyeColor, other.EyeColor)) return false;
      if (ExpressionTriggerId != other.ExpressionTriggerId) return false;
      if (ExpressionTriggerTimestamp != other.ExpressionTriggerTimestamp) return false;
      if (Talking != other.Talking) return false;
      if(!wearables_.Equals(other.wearables_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (Id.Length != 0) hash ^= Id.GetHashCode();
      if (HasName) hash ^= Name.GetHashCode();
      if (HasBodyShape) hash ^= BodyShape.GetHashCode();
      if (skinColor_ != null) hash ^= SkinColor.GetHashCode();
      if (hairColor_ != null) hash ^= HairColor.GetHashCode();
      if (eyeColor_ != null) hash ^= EyeColor.GetHashCode();
      if (HasExpressionTriggerId) hash ^= ExpressionTriggerId.GetHashCode();
      if (HasExpressionTriggerTimestamp) hash ^= ExpressionTriggerTimestamp.GetHashCode();
      if (HasTalking) hash ^= Talking.GetHashCode();
      hash ^= wearables_.GetHashCode();
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
      if (Id.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Id);
      }
      if (HasName) {
        output.WriteRawTag(18);
        output.WriteString(Name);
      }
      if (HasBodyShape) {
        output.WriteRawTag(26);
        output.WriteString(BodyShape);
      }
      if (skinColor_ != null) {
        output.WriteRawTag(34);
        output.WriteMessage(SkinColor);
      }
      if (hairColor_ != null) {
        output.WriteRawTag(42);
        output.WriteMessage(HairColor);
      }
      if (eyeColor_ != null) {
        output.WriteRawTag(50);
        output.WriteMessage(EyeColor);
      }
      if (HasExpressionTriggerId) {
        output.WriteRawTag(58);
        output.WriteString(ExpressionTriggerId);
      }
      if (HasExpressionTriggerTimestamp) {
        output.WriteRawTag(64);
        output.WriteInt64(ExpressionTriggerTimestamp);
      }
      if (HasTalking) {
        output.WriteRawTag(72);
        output.WriteBool(Talking);
      }
      wearables_.WriteTo(output, _repeated_wearables_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (Id.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Id);
      }
      if (HasName) {
        output.WriteRawTag(18);
        output.WriteString(Name);
      }
      if (HasBodyShape) {
        output.WriteRawTag(26);
        output.WriteString(BodyShape);
      }
      if (skinColor_ != null) {
        output.WriteRawTag(34);
        output.WriteMessage(SkinColor);
      }
      if (hairColor_ != null) {
        output.WriteRawTag(42);
        output.WriteMessage(HairColor);
      }
      if (eyeColor_ != null) {
        output.WriteRawTag(50);
        output.WriteMessage(EyeColor);
      }
      if (HasExpressionTriggerId) {
        output.WriteRawTag(58);
        output.WriteString(ExpressionTriggerId);
      }
      if (HasExpressionTriggerTimestamp) {
        output.WriteRawTag(64);
        output.WriteInt64(ExpressionTriggerTimestamp);
      }
      if (HasTalking) {
        output.WriteRawTag(72);
        output.WriteBool(Talking);
      }
      wearables_.WriteTo(ref output, _repeated_wearables_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (Id.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Id);
      }
      if (HasName) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
      }
      if (HasBodyShape) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(BodyShape);
      }
      if (skinColor_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(SkinColor);
      }
      if (hairColor_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(HairColor);
      }
      if (eyeColor_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(EyeColor);
      }
      if (HasExpressionTriggerId) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ExpressionTriggerId);
      }
      if (HasExpressionTriggerTimestamp) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(ExpressionTriggerTimestamp);
      }
      if (HasTalking) {
        size += 1 + 1;
      }
      size += wearables_.CalculateSize(_repeated_wearables_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(PBAvatarShape other) {
      if (other == null) {
        return;
      }
      if (other.Id.Length != 0) {
        Id = other.Id;
      }
      if (other.HasName) {
        Name = other.Name;
      }
      if (other.HasBodyShape) {
        BodyShape = other.BodyShape;
      }
      if (other.skinColor_ != null) {
        if (skinColor_ == null) {
          SkinColor = new global::DCL.ECSComponents.Color3();
        }
        SkinColor.MergeFrom(other.SkinColor);
      }
      if (other.hairColor_ != null) {
        if (hairColor_ == null) {
          HairColor = new global::DCL.ECSComponents.Color3();
        }
        HairColor.MergeFrom(other.HairColor);
      }
      if (other.eyeColor_ != null) {
        if (eyeColor_ == null) {
          EyeColor = new global::DCL.ECSComponents.Color3();
        }
        EyeColor.MergeFrom(other.EyeColor);
      }
      if (other.HasExpressionTriggerId) {
        ExpressionTriggerId = other.ExpressionTriggerId;
      }
      if (other.HasExpressionTriggerTimestamp) {
        ExpressionTriggerTimestamp = other.ExpressionTriggerTimestamp;
      }
      if (other.HasTalking) {
        Talking = other.Talking;
      }
      wearables_.Add(other.wearables_);
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
            Id = input.ReadString();
            break;
          }
          case 18: {
            Name = input.ReadString();
            break;
          }
          case 26: {
            BodyShape = input.ReadString();
            break;
          }
          case 34: {
            if (skinColor_ == null) {
              SkinColor = new global::DCL.ECSComponents.Color3();
            }
            input.ReadMessage(SkinColor);
            break;
          }
          case 42: {
            if (hairColor_ == null) {
              HairColor = new global::DCL.ECSComponents.Color3();
            }
            input.ReadMessage(HairColor);
            break;
          }
          case 50: {
            if (eyeColor_ == null) {
              EyeColor = new global::DCL.ECSComponents.Color3();
            }
            input.ReadMessage(EyeColor);
            break;
          }
          case 58: {
            ExpressionTriggerId = input.ReadString();
            break;
          }
          case 64: {
            ExpressionTriggerTimestamp = input.ReadInt64();
            break;
          }
          case 72: {
            Talking = input.ReadBool();
            break;
          }
          case 82: {
            wearables_.AddEntriesFrom(input, _repeated_wearables_codec);
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
            Id = input.ReadString();
            break;
          }
          case 18: {
            Name = input.ReadString();
            break;
          }
          case 26: {
            BodyShape = input.ReadString();
            break;
          }
          case 34: {
            if (skinColor_ == null) {
              SkinColor = new global::DCL.ECSComponents.Color3();
            }
            input.ReadMessage(SkinColor);
            break;
          }
          case 42: {
            if (hairColor_ == null) {
              HairColor = new global::DCL.ECSComponents.Color3();
            }
            input.ReadMessage(HairColor);
            break;
          }
          case 50: {
            if (eyeColor_ == null) {
              EyeColor = new global::DCL.ECSComponents.Color3();
            }
            input.ReadMessage(EyeColor);
            break;
          }
          case 58: {
            ExpressionTriggerId = input.ReadString();
            break;
          }
          case 64: {
            ExpressionTriggerTimestamp = input.ReadInt64();
            break;
          }
          case 72: {
            Talking = input.ReadBool();
            break;
          }
          case 82: {
            wearables_.AddEntriesFrom(ref input, _repeated_wearables_codec);
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
