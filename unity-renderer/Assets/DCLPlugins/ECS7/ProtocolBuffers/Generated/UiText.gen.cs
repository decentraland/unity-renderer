// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: decentraland/sdk/components/ui_text.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace DCL.ECSComponents {

  /// <summary>Holder for reflection information generated from decentraland/sdk/components/ui_text.proto</summary>
  public static partial class UiTextReflection {

    #region Descriptor
    /// <summary>File descriptor for decentraland/sdk/components/ui_text.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static UiTextReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CilkZWNlbnRyYWxhbmQvc2RrL2NvbXBvbmVudHMvdWlfdGV4dC5wcm90bxIb",
            "ZGVjZW50cmFsYW5kLnNkay5jb21wb25lbnRzGiBkZWNlbnRyYWxhbmQvY29t",
            "bW9uL2NvbG9ycy5wcm90bxouZGVjZW50cmFsYW5kL3Nkay9jb21wb25lbnRz",
            "L2NvbW1vbi90ZXh0cy5wcm90byKbAgoIUEJVaVRleHQSDQoFdmFsdWUYASAB",
            "KAkSLwoFY29sb3IYAiABKAsyGy5kZWNlbnRyYWxhbmQuY29tbW9uLkNvbG9y",
            "NEgAiAEBEkoKCnRleHRfYWxpZ24YAyABKA4yMS5kZWNlbnRyYWxhbmQuc2Rr",
            "LmNvbXBvbmVudHMuY29tbW9uLlRleHRBbGlnbk1vZGVIAYgBARI7CgRmb250",
            "GAQgASgOMiguZGVjZW50cmFsYW5kLnNkay5jb21wb25lbnRzLmNvbW1vbi5G",
            "b250SAKIAQESFgoJZm9udF9zaXplGAUgASgFSAOIAQFCCAoGX2NvbG9yQg0K",
            "C190ZXh0X2FsaWduQgcKBV9mb250QgwKCl9mb250X3NpemVCFKoCEURDTC5F",
            "Q1NDb21wb25lbnRzYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::DCL.ECSComponents.ColorsReflection.Descriptor, global::DCL.ECSComponents.TextsReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::DCL.ECSComponents.PBUiText), global::DCL.ECSComponents.PBUiText.Parser, new[]{ "Value", "Color", "TextAlign", "Font", "FontSize" }, new[]{ "Color", "TextAlign", "Font", "FontSize" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class PBUiText : pb::IMessage<PBUiText>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<PBUiText> _parser = new pb::MessageParser<PBUiText>(() => new PBUiText());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<PBUiText> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::DCL.ECSComponents.UiTextReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBUiText() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBUiText(PBUiText other) : this() {
      _hasBits0 = other._hasBits0;
      value_ = other.value_;
      color_ = other.color_ != null ? other.color_.Clone() : null;
      textAlign_ = other.textAlign_;
      font_ = other.font_;
      fontSize_ = other.fontSize_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBUiText Clone() {
      return new PBUiText(this);
    }

    /// <summary>Field number for the "value" field.</summary>
    public const int ValueFieldNumber = 1;
    private string value_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string Value {
      get { return value_; }
      set {
        value_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "color" field.</summary>
    public const int ColorFieldNumber = 2;
    private global::DCL.ECSComponents.Color4 color_;
    /// <summary>
    /// default=(1.0,1.0,1.0,1.0)
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::DCL.ECSComponents.Color4 Color {
      get { return color_; }
      set {
        color_ = value;
      }
    }

    /// <summary>Field number for the "text_align" field.</summary>
    public const int TextAlignFieldNumber = 3;
    private global::DCL.ECSComponents.TextAlignMode textAlign_;
    /// <summary>
    /// default='center'
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::DCL.ECSComponents.TextAlignMode TextAlign {
      get { if ((_hasBits0 & 1) != 0) { return textAlign_; } else { return global::DCL.ECSComponents.TextAlignMode.TamTopLeft; } }
      set {
        _hasBits0 |= 1;
        textAlign_ = value;
      }
    }
    /// <summary>Gets whether the "text_align" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasTextAlign {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "text_align" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearTextAlign() {
      _hasBits0 &= ~1;
    }

    /// <summary>Field number for the "font" field.</summary>
    public const int FontFieldNumber = 4;
    private global::DCL.ECSComponents.Font font_;
    /// <summary>
    /// default=0
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::DCL.ECSComponents.Font Font {
      get { if ((_hasBits0 & 2) != 0) { return font_; } else { return global::DCL.ECSComponents.Font.FSansSerif; } }
      set {
        _hasBits0 |= 2;
        font_ = value;
      }
    }
    /// <summary>Gets whether the "font" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasFont {
      get { return (_hasBits0 & 2) != 0; }
    }
    /// <summary>Clears the value of the "font" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearFont() {
      _hasBits0 &= ~2;
    }

    /// <summary>Field number for the "font_size" field.</summary>
    public const int FontSizeFieldNumber = 5;
    private int fontSize_;
    /// <summary>
    /// default=10
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int FontSize {
      get { if ((_hasBits0 & 4) != 0) { return fontSize_; } else { return 0; } }
      set {
        _hasBits0 |= 4;
        fontSize_ = value;
      }
    }
    /// <summary>Gets whether the "font_size" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasFontSize {
      get { return (_hasBits0 & 4) != 0; }
    }
    /// <summary>Clears the value of the "font_size" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearFontSize() {
      _hasBits0 &= ~4;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as PBUiText);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(PBUiText other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Value != other.Value) return false;
      if (!object.Equals(Color, other.Color)) return false;
      if (TextAlign != other.TextAlign) return false;
      if (Font != other.Font) return false;
      if (FontSize != other.FontSize) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (Value.Length != 0) hash ^= Value.GetHashCode();
      if (color_ != null) hash ^= Color.GetHashCode();
      if (HasTextAlign) hash ^= TextAlign.GetHashCode();
      if (HasFont) hash ^= Font.GetHashCode();
      if (HasFontSize) hash ^= FontSize.GetHashCode();
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
      if (Value.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Value);
      }
      if (color_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(Color);
      }
      if (HasTextAlign) {
        output.WriteRawTag(24);
        output.WriteEnum((int) TextAlign);
      }
      if (HasFont) {
        output.WriteRawTag(32);
        output.WriteEnum((int) Font);
      }
      if (HasFontSize) {
        output.WriteRawTag(40);
        output.WriteInt32(FontSize);
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
      if (Value.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Value);
      }
      if (color_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(Color);
      }
      if (HasTextAlign) {
        output.WriteRawTag(24);
        output.WriteEnum((int) TextAlign);
      }
      if (HasFont) {
        output.WriteRawTag(32);
        output.WriteEnum((int) Font);
      }
      if (HasFontSize) {
        output.WriteRawTag(40);
        output.WriteInt32(FontSize);
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
      if (Value.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Value);
      }
      if (color_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Color);
      }
      if (HasTextAlign) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) TextAlign);
      }
      if (HasFont) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Font);
      }
      if (HasFontSize) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(FontSize);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(PBUiText other) {
      if (other == null) {
        return;
      }
      if (other.Value.Length != 0) {
        Value = other.Value;
      }
      if (other.color_ != null) {
        if (color_ == null) {
          Color = new global::DCL.ECSComponents.Color4();
        }
        Color.MergeFrom(other.Color);
      }
      if (other.HasTextAlign) {
        TextAlign = other.TextAlign;
      }
      if (other.HasFont) {
        Font = other.Font;
      }
      if (other.HasFontSize) {
        FontSize = other.FontSize;
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
            Value = input.ReadString();
            break;
          }
          case 18: {
            if (color_ == null) {
              Color = new global::DCL.ECSComponents.Color4();
            }
            input.ReadMessage(Color);
            break;
          }
          case 24: {
            TextAlign = (global::DCL.ECSComponents.TextAlignMode) input.ReadEnum();
            break;
          }
          case 32: {
            Font = (global::DCL.ECSComponents.Font) input.ReadEnum();
            break;
          }
          case 40: {
            FontSize = input.ReadInt32();
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
            Value = input.ReadString();
            break;
          }
          case 18: {
            if (color_ == null) {
              Color = new global::DCL.ECSComponents.Color4();
            }
            input.ReadMessage(Color);
            break;
          }
          case 24: {
            TextAlign = (global::DCL.ECSComponents.TextAlignMode) input.ReadEnum();
            break;
          }
          case 32: {
            Font = (global::DCL.ECSComponents.Font) input.ReadEnum();
            break;
          }
          case 40: {
            FontSize = input.ReadInt32();
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
