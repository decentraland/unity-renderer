// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: decentraland/sdk/components/ui_dropdown.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace DCL.ECSComponents {

  /// <summary>Holder for reflection information generated from decentraland/sdk/components/ui_dropdown.proto</summary>
  public static partial class UiDropdownReflection {

    #region Descriptor
    /// <summary>File descriptor for decentraland/sdk/components/ui_dropdown.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static UiDropdownReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Ci1kZWNlbnRyYWxhbmQvc2RrL2NvbXBvbmVudHMvdWlfZHJvcGRvd24ucHJv",
            "dG8SG2RlY2VudHJhbGFuZC5zZGsuY29tcG9uZW50cxogZGVjZW50cmFsYW5k",
            "L2NvbW1vbi9jb2xvcnMucHJvdG8aLmRlY2VudHJhbGFuZC9zZGsvY29tcG9u",
            "ZW50cy9jb21tb24vdGV4dHMucHJvdG8iowMKDFBCVWlEcm9wZG93bhIUCgxh",
            "Y2NlcHRfZW1wdHkYASABKAgSGAoLZW1wdHlfbGFiZWwYAiABKAlIAIgBARIP",
            "CgdvcHRpb25zGAMgAygJEhsKDnNlbGVjdGVkX2luZGV4GAQgASgFSAGIAQES",
            "EAoIZGlzYWJsZWQYBSABKAgSLwoFY29sb3IYBiABKAsyGy5kZWNlbnRyYWxh",
            "bmQuY29tbW9uLkNvbG9yNEgCiAEBEkoKCnRleHRfYWxpZ24YCiABKA4yMS5k",
            "ZWNlbnRyYWxhbmQuc2RrLmNvbXBvbmVudHMuY29tbW9uLlRleHRBbGlnbk1v",
            "ZGVIA4gBARI7CgRmb250GAsgASgOMiguZGVjZW50cmFsYW5kLnNkay5jb21w",
            "b25lbnRzLmNvbW1vbi5Gb250SASIAQESFgoJZm9udF9zaXplGAwgASgFSAWI",
            "AQFCDgoMX2VtcHR5X2xhYmVsQhEKD19zZWxlY3RlZF9pbmRleEIICgZfY29s",
            "b3JCDQoLX3RleHRfYWxpZ25CBwoFX2ZvbnRCDAoKX2ZvbnRfc2l6ZUIUqgIR",
            "RENMLkVDU0NvbXBvbmVudHNiBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::DCL.ECSComponents.ColorsReflection.Descriptor, global::DCL.ECSComponents.TextsReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::DCL.ECSComponents.PBUiDropdown), global::DCL.ECSComponents.PBUiDropdown.Parser, new[]{ "AcceptEmpty", "EmptyLabel", "Options", "SelectedIndex", "Disabled", "Color", "TextAlign", "Font", "FontSize" }, new[]{ "EmptyLabel", "SelectedIndex", "Color", "TextAlign", "Font", "FontSize" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class PBUiDropdown : pb::IMessage<PBUiDropdown>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<PBUiDropdown> _parser = new pb::MessageParser<PBUiDropdown>(() => new PBUiDropdown());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<PBUiDropdown> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::DCL.ECSComponents.UiDropdownReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBUiDropdown() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBUiDropdown(PBUiDropdown other) : this() {
      _hasBits0 = other._hasBits0;
      acceptEmpty_ = other.acceptEmpty_;
      emptyLabel_ = other.emptyLabel_;
      options_ = other.options_.Clone();
      selectedIndex_ = other.selectedIndex_;
      disabled_ = other.disabled_;
      color_ = other.color_ != null ? other.color_.Clone() : null;
      textAlign_ = other.textAlign_;
      font_ = other.font_;
      fontSize_ = other.fontSize_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBUiDropdown Clone() {
      return new PBUiDropdown(this);
    }

    /// <summary>Field number for the "accept_empty" field.</summary>
    public const int AcceptEmptyFieldNumber = 1;
    private bool acceptEmpty_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool AcceptEmpty {
      get { return acceptEmpty_; }
      set {
        acceptEmpty_ = value;
      }
    }

    /// <summary>Field number for the "empty_label" field.</summary>
    public const int EmptyLabelFieldNumber = 2;
    private string emptyLabel_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string EmptyLabel {
      get { return emptyLabel_ ?? ""; }
      set {
        emptyLabel_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }
    /// <summary>Gets whether the "empty_label" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasEmptyLabel {
      get { return emptyLabel_ != null; }
    }
    /// <summary>Clears the value of the "empty_label" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearEmptyLabel() {
      emptyLabel_ = null;
    }

    /// <summary>Field number for the "options" field.</summary>
    public const int OptionsFieldNumber = 3;
    private static readonly pb::FieldCodec<string> _repeated_options_codec
        = pb::FieldCodec.ForString(26);
    private readonly pbc::RepeatedField<string> options_ = new pbc::RepeatedField<string>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<string> Options {
      get { return options_; }
    }

    /// <summary>Field number for the "selected_index" field.</summary>
    public const int SelectedIndexFieldNumber = 4;
    private int selectedIndex_;
    /// <summary>
    /// default=-1 when accept_empty==true; default=0 when accept_empty==false
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int SelectedIndex {
      get { if ((_hasBits0 & 1) != 0) { return selectedIndex_; } else { return 0; } }
      set {
        _hasBits0 |= 1;
        selectedIndex_ = value;
      }
    }
    /// <summary>Gets whether the "selected_index" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasSelectedIndex {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "selected_index" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearSelectedIndex() {
      _hasBits0 &= ~1;
    }

    /// <summary>Field number for the "disabled" field.</summary>
    public const int DisabledFieldNumber = 5;
    private bool disabled_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Disabled {
      get { return disabled_; }
      set {
        disabled_ = value;
      }
    }

    /// <summary>Field number for the "color" field.</summary>
    public const int ColorFieldNumber = 6;
    private global::DCL.ECSComponents.Color4 color_;
    /// <summary>
    /// default=(0.0,0.0,0.0,1.0)
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
    public const int TextAlignFieldNumber = 10;
    private global::DCL.ECSComponents.TextAlignMode textAlign_;
    /// <summary>
    /// default='center'
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::DCL.ECSComponents.TextAlignMode TextAlign {
      get { if ((_hasBits0 & 2) != 0) { return textAlign_; } else { return global::DCL.ECSComponents.TextAlignMode.TamTopLeft; } }
      set {
        _hasBits0 |= 2;
        textAlign_ = value;
      }
    }
    /// <summary>Gets whether the "text_align" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasTextAlign {
      get { return (_hasBits0 & 2) != 0; }
    }
    /// <summary>Clears the value of the "text_align" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearTextAlign() {
      _hasBits0 &= ~2;
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
      get { if ((_hasBits0 & 4) != 0) { return font_; } else { return global::DCL.ECSComponents.Font.FSansSerif; } }
      set {
        _hasBits0 |= 4;
        font_ = value;
      }
    }
    /// <summary>Gets whether the "font" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasFont {
      get { return (_hasBits0 & 4) != 0; }
    }
    /// <summary>Clears the value of the "font" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearFont() {
      _hasBits0 &= ~4;
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
      get { if ((_hasBits0 & 8) != 0) { return fontSize_; } else { return 0; } }
      set {
        _hasBits0 |= 8;
        fontSize_ = value;
      }
    }
    /// <summary>Gets whether the "font_size" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasFontSize {
      get { return (_hasBits0 & 8) != 0; }
    }
    /// <summary>Clears the value of the "font_size" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearFontSize() {
      _hasBits0 &= ~8;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as PBUiDropdown);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(PBUiDropdown other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (AcceptEmpty != other.AcceptEmpty) return false;
      if (EmptyLabel != other.EmptyLabel) return false;
      if(!options_.Equals(other.options_)) return false;
      if (SelectedIndex != other.SelectedIndex) return false;
      if (Disabled != other.Disabled) return false;
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
      if (AcceptEmpty != false) hash ^= AcceptEmpty.GetHashCode();
      if (HasEmptyLabel) hash ^= EmptyLabel.GetHashCode();
      hash ^= options_.GetHashCode();
      if (HasSelectedIndex) hash ^= SelectedIndex.GetHashCode();
      if (Disabled != false) hash ^= Disabled.GetHashCode();
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
      if (AcceptEmpty != false) {
        output.WriteRawTag(8);
        output.WriteBool(AcceptEmpty);
      }
      if (HasEmptyLabel) {
        output.WriteRawTag(18);
        output.WriteString(EmptyLabel);
      }
      options_.WriteTo(output, _repeated_options_codec);
      if (HasSelectedIndex) {
        output.WriteRawTag(32);
        output.WriteInt32(SelectedIndex);
      }
      if (Disabled != false) {
        output.WriteRawTag(40);
        output.WriteBool(Disabled);
      }
      if (color_ != null) {
        output.WriteRawTag(50);
        output.WriteMessage(Color);
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
      if (AcceptEmpty != false) {
        output.WriteRawTag(8);
        output.WriteBool(AcceptEmpty);
      }
      if (HasEmptyLabel) {
        output.WriteRawTag(18);
        output.WriteString(EmptyLabel);
      }
      options_.WriteTo(ref output, _repeated_options_codec);
      if (HasSelectedIndex) {
        output.WriteRawTag(32);
        output.WriteInt32(SelectedIndex);
      }
      if (Disabled != false) {
        output.WriteRawTag(40);
        output.WriteBool(Disabled);
      }
      if (color_ != null) {
        output.WriteRawTag(50);
        output.WriteMessage(Color);
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
      if (AcceptEmpty != false) {
        size += 1 + 1;
      }
      if (HasEmptyLabel) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(EmptyLabel);
      }
      size += options_.CalculateSize(_repeated_options_codec);
      if (HasSelectedIndex) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(SelectedIndex);
      }
      if (Disabled != false) {
        size += 1 + 1;
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
    public void MergeFrom(PBUiDropdown other) {
      if (other == null) {
        return;
      }
      if (other.AcceptEmpty != false) {
        AcceptEmpty = other.AcceptEmpty;
      }
      if (other.HasEmptyLabel) {
        EmptyLabel = other.EmptyLabel;
      }
      options_.Add(other.options_);
      if (other.HasSelectedIndex) {
        SelectedIndex = other.SelectedIndex;
      }
      if (other.Disabled != false) {
        Disabled = other.Disabled;
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
          case 8: {
            AcceptEmpty = input.ReadBool();
            break;
          }
          case 18: {
            EmptyLabel = input.ReadString();
            break;
          }
          case 26: {
            options_.AddEntriesFrom(input, _repeated_options_codec);
            break;
          }
          case 32: {
            SelectedIndex = input.ReadInt32();
            break;
          }
          case 40: {
            Disabled = input.ReadBool();
            break;
          }
          case 50: {
            if (color_ == null) {
              Color = new global::DCL.ECSComponents.Color4();
            }
            input.ReadMessage(Color);
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
          case 8: {
            AcceptEmpty = input.ReadBool();
            break;
          }
          case 18: {
            EmptyLabel = input.ReadString();
            break;
          }
          case 26: {
            options_.AddEntriesFrom(ref input, _repeated_options_codec);
            break;
          }
          case 32: {
            SelectedIndex = input.ReadInt32();
            break;
          }
          case 40: {
            Disabled = input.ReadBool();
            break;
          }
          case 50: {
            if (color_ == null) {
              Color = new global::DCL.ECSComponents.Color4();
            }
            input.ReadMessage(Color);
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
