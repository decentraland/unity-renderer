// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: decentraland/sdk/components/ui_input.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace DCL.ECSComponents {

  /// <summary>Holder for reflection information generated from decentraland/sdk/components/ui_input.proto</summary>
  public static partial class UiInputReflection {

    #region Descriptor
    /// <summary>File descriptor for decentraland/sdk/components/ui_input.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static UiInputReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CipkZWNlbnRyYWxhbmQvc2RrL2NvbXBvbmVudHMvdWlfaW5wdXQucHJvdG8S",
            "G2RlY2VudHJhbGFuZC5zZGsuY29tcG9uZW50cxogZGVjZW50cmFsYW5kL2Nv",
            "bW1vbi9jb2xvcnMucHJvdG8aLmRlY2VudHJhbGFuZC9zZGsvY29tcG9uZW50",
            "cy9jb21tb24vdGV4dHMucHJvdG8ihwMKCVBCVWlJbnB1dBITCgtwbGFjZWhv",
            "bGRlchgBIAEoCRIvCgVjb2xvchgCIAEoCzIbLmRlY2VudHJhbGFuZC5jb21t",
            "b24uQ29sb3I0SACIAQESOwoRcGxhY2Vob2xkZXJfY29sb3IYAyABKAsyGy5k",
            "ZWNlbnRyYWxhbmQuY29tbW9uLkNvbG9yNEgBiAEBEhAKCGRpc2FibGVkGAQg",
            "ASgIEkoKCnRleHRfYWxpZ24YCiABKA4yMS5kZWNlbnRyYWxhbmQuc2RrLmNv",
            "bXBvbmVudHMuY29tbW9uLlRleHRBbGlnbk1vZGVIAogBARI7CgRmb250GAsg",
            "ASgOMiguZGVjZW50cmFsYW5kLnNkay5jb21wb25lbnRzLmNvbW1vbi5Gb250",
            "SAOIAQESFgoJZm9udF9zaXplGAwgASgFSASIAQFCCAoGX2NvbG9yQhQKEl9w",
            "bGFjZWhvbGRlcl9jb2xvckINCgtfdGV4dF9hbGlnbkIHCgVfZm9udEIMCgpf",
            "Zm9udF9zaXplQhSqAhFEQ0wuRUNTQ29tcG9uZW50c2IGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Decentraland.Common.ColorsReflection.Descriptor, global::DCL.ECSComponents.TextsReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::DCL.ECSComponents.PBUiInput), global::DCL.ECSComponents.PBUiInput.Parser, new[]{ "Placeholder", "Color", "PlaceholderColor", "Disabled", "TextAlign", "Font", "FontSize" }, new[]{ "Color", "PlaceholderColor", "TextAlign", "Font", "FontSize" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class PBUiInput : pb::IMessage<PBUiInput>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<PBUiInput> _parser = new pb::MessageParser<PBUiInput>(() => new PBUiInput());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<PBUiInput> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::DCL.ECSComponents.UiInputReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBUiInput() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBUiInput(PBUiInput other) : this() {
      _hasBits0 = other._hasBits0;
      placeholder_ = other.placeholder_;
      color_ = other.color_ != null ? other.color_.Clone() : null;
      placeholderColor_ = other.placeholderColor_ != null ? other.placeholderColor_.Clone() : null;
      disabled_ = other.disabled_;
      textAlign_ = other.textAlign_;
      font_ = other.font_;
      fontSize_ = other.fontSize_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBUiInput Clone() {
      return new PBUiInput(this);
    }

    /// <summary>Field number for the "placeholder" field.</summary>
    public const int PlaceholderFieldNumber = 1;
    private string placeholder_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string Placeholder {
      get { return placeholder_; }
      set {
        placeholder_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "color" field.</summary>
    public const int ColorFieldNumber = 2;
    private global::Decentraland.Common.Color4 color_;
    /// <summary>
    /// default=(0.0,0.0,0.0,1.0)
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Decentraland.Common.Color4 Color {
      get { return color_; }
      set {
        color_ = value;
      }
    }

    /// <summary>Field number for the "placeholder_color" field.</summary>
    public const int PlaceholderColorFieldNumber = 3;
    private global::Decentraland.Common.Color4 placeholderColor_;
    /// <summary>
    /// default=(0.3,0.3,0.3,1.0)
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Decentraland.Common.Color4 PlaceholderColor {
      get { return placeholderColor_; }
      set {
        placeholderColor_ = value;
      }
    }

    /// <summary>Field number for the "disabled" field.</summary>
    public const int DisabledFieldNumber = 4;
    private bool disabled_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Disabled {
      get { return disabled_; }
      set {
        disabled_ = value;
      }
    }

    /// <summary>Field number for the "text_align" field.</summary>
    public const int TextAlignFieldNumber = 10;
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
    public const int FontFieldNumber = 11;
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
    public const int FontSizeFieldNumber = 12;
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
      return Equals(other as PBUiInput);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(PBUiInput other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Placeholder != other.Placeholder) return false;
      if (!object.Equals(Color, other.Color)) return false;
      if (!object.Equals(PlaceholderColor, other.PlaceholderColor)) return false;
      if (Disabled != other.Disabled) return false;
      if (TextAlign != other.TextAlign) return false;
      if (Font != other.Font) return false;
      if (FontSize != other.FontSize) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (Placeholder.Length != 0) hash ^= Placeholder.GetHashCode();
      if (color_ != null) hash ^= Color.GetHashCode();
      if (placeholderColor_ != null) hash ^= PlaceholderColor.GetHashCode();
      if (Disabled != false) hash ^= Disabled.GetHashCode();
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
      if (Placeholder.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Placeholder);
      }
      if (color_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(Color);
      }
      if (placeholderColor_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(PlaceholderColor);
      }
      if (Disabled != false) {
        output.WriteRawTag(32);
        output.WriteBool(Disabled);
      }
      if (HasTextAlign) {
        output.WriteRawTag(80);
        output.WriteEnum((int) TextAlign);
      }
      if (HasFont) {
        output.WriteRawTag(88);
        output.WriteEnum((int) Font);
      }
      if (HasFontSize) {
        output.WriteRawTag(96);
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
      if (Placeholder.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Placeholder);
      }
      if (color_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(Color);
      }
      if (placeholderColor_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(PlaceholderColor);
      }
      if (Disabled != false) {
        output.WriteRawTag(32);
        output.WriteBool(Disabled);
      }
      if (HasTextAlign) {
        output.WriteRawTag(80);
        output.WriteEnum((int) TextAlign);
      }
      if (HasFont) {
        output.WriteRawTag(88);
        output.WriteEnum((int) Font);
      }
      if (HasFontSize) {
        output.WriteRawTag(96);
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
      if (Placeholder.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Placeholder);
      }
      if (color_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Color);
      }
      if (placeholderColor_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(PlaceholderColor);
      }
      if (Disabled != false) {
        size += 1 + 1;
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
    public void MergeFrom(PBUiInput other) {
      if (other == null) {
        return;
      }
      if (other.Placeholder.Length != 0) {
        Placeholder = other.Placeholder;
      }
      if (other.color_ != null) {
        if (color_ == null) {
          Color = new global::Decentraland.Common.Color4();
        }
        Color.MergeFrom(other.Color);
      }
      if (other.placeholderColor_ != null) {
        if (placeholderColor_ == null) {
          PlaceholderColor = new global::Decentraland.Common.Color4();
        }
        PlaceholderColor.MergeFrom(other.PlaceholderColor);
      }
      if (other.Disabled != false) {
        Disabled = other.Disabled;
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
            Placeholder = input.ReadString();
            break;
          }
          case 18: {
            if (color_ == null) {
              Color = new global::Decentraland.Common.Color4();
            }
            input.ReadMessage(Color);
            break;
          }
          case 26: {
            if (placeholderColor_ == null) {
              PlaceholderColor = new global::Decentraland.Common.Color4();
            }
            input.ReadMessage(PlaceholderColor);
            break;
          }
          case 32: {
            Disabled = input.ReadBool();
            break;
          }
          case 80: {
            TextAlign = (global::DCL.ECSComponents.TextAlignMode) input.ReadEnum();
            break;
          }
          case 88: {
            Font = (global::DCL.ECSComponents.Font) input.ReadEnum();
            break;
          }
          case 96: {
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
            Placeholder = input.ReadString();
            break;
          }
          case 18: {
            if (color_ == null) {
              Color = new global::Decentraland.Common.Color4();
            }
            input.ReadMessage(Color);
            break;
          }
          case 26: {
            if (placeholderColor_ == null) {
              PlaceholderColor = new global::Decentraland.Common.Color4();
            }
            input.ReadMessage(PlaceholderColor);
            break;
          }
          case 32: {
            Disabled = input.ReadBool();
            break;
          }
          case 80: {
            TextAlign = (global::DCL.ECSComponents.TextAlignMode) input.ReadEnum();
            break;
          }
          case 88: {
            Font = (global::DCL.ECSComponents.Font) input.ReadEnum();
            break;
          }
          case 96: {
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
