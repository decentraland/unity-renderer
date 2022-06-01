// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: TextShape.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace DCL.ECSComponents {

  /// <summary>Holder for reflection information generated from TextShape.proto</summary>
  public static partial class TextShapeReflection {

    #region Descriptor
    /// <summary>File descriptor for TextShape.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static TextShapeReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cg9UZXh0U2hhcGUucHJvdG8SEGRlY2VudHJhbGFuZC5lY3MirwQKC1BCVGV4",
            "dFNoYXBlEgwKBHRleHQYASABKAkSDwoHdmlzaWJsZRgCIAEoCBIMCgRmb250",
            "GAMgASgJEg8KB29wYWNpdHkYBCABKAISEAoIZm9udFNpemUYBSABKAISFAoM",
            "Zm9udEF1dG9TaXplGAYgASgIEhIKCmhUZXh0QWxpZ24YByABKAkSEgoKdlRl",
            "eHRBbGlnbhgIIAEoCRINCgV3aWR0aBgJIAEoAhIOCgZoZWlnaHQYCiABKAIS",
            "EgoKcGFkZGluZ1RvcBgLIAEoAhIUCgxwYWRkaW5nUmlnaHQYDCABKAISFQoN",
            "cGFkZGluZ0JvdHRvbRgNIAEoAhITCgtwYWRkaW5nTGVmdBgOIAEoAhITCgts",
            "aW5lU3BhY2luZxgPIAEoAhIRCglsaW5lQ291bnQYECABKAUSFAoMdGV4dFdy",
            "YXBwaW5nGBEgASgIEhIKCnNoYWRvd0JsdXIYEiABKAISFQoNc2hhZG93T2Zm",
            "c2V0WBgTIAEoAhIVCg1zaGFkb3dPZmZzZXRZGBQgASgCEhQKDG91dGxpbmVX",
            "aWR0aBgVIAEoAhIsCgtzaGFkb3dDb2xvchgWIAEoCzIXLmRlY2VudHJhbGFu",
            "ZC5lY3MuQ29sb3ISLQoMb3V0bGluZUNvbG9yGBcgASgLMhcuZGVjZW50cmFs",
            "YW5kLmVjcy5Db2xvchIqCgl0ZXh0Q29sb3IYGCABKAsyFy5kZWNlbnRyYWxh",
            "bmQuZWNzLkNvbG9yIjEKBUNvbG9yEgsKA3JlZBgBIAEoAhINCgVncmVlbhgC",
            "IAEoAhIMCgRibHVlGAMgASgCQhSqAhFEQ0wuRUNTQ29tcG9uZW50c2IGcHJv",
            "dG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::DCL.ECSComponents.PBTextShape), global::DCL.ECSComponents.PBTextShape.Parser, new[]{ "Text", "Visible", "Font", "Opacity", "FontSize", "FontAutoSize", "HTextAlign", "VTextAlign", "Width", "Height", "PaddingTop", "PaddingRight", "PaddingBottom", "PaddingLeft", "LineSpacing", "LineCount", "TextWrapping", "ShadowBlur", "ShadowOffsetX", "ShadowOffsetY", "OutlineWidth", "ShadowColor", "OutlineColor", "TextColor" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::DCL.ECSComponents.Color), global::DCL.ECSComponents.Color.Parser, new[]{ "Red", "Green", "Blue" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class PBTextShape : pb::IMessage<PBTextShape> {
    private static readonly pb::MessageParser<PBTextShape> _parser = new pb::MessageParser<PBTextShape>(() => new PBTextShape());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<PBTextShape> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::DCL.ECSComponents.TextShapeReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public PBTextShape() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public PBTextShape(PBTextShape other) : this() {
      text_ = other.text_;
      visible_ = other.visible_;
      font_ = other.font_;
      opacity_ = other.opacity_;
      fontSize_ = other.fontSize_;
      fontAutoSize_ = other.fontAutoSize_;
      hTextAlign_ = other.hTextAlign_;
      vTextAlign_ = other.vTextAlign_;
      width_ = other.width_;
      height_ = other.height_;
      paddingTop_ = other.paddingTop_;
      paddingRight_ = other.paddingRight_;
      paddingBottom_ = other.paddingBottom_;
      paddingLeft_ = other.paddingLeft_;
      lineSpacing_ = other.lineSpacing_;
      lineCount_ = other.lineCount_;
      textWrapping_ = other.textWrapping_;
      shadowBlur_ = other.shadowBlur_;
      shadowOffsetX_ = other.shadowOffsetX_;
      shadowOffsetY_ = other.shadowOffsetY_;
      outlineWidth_ = other.outlineWidth_;
      shadowColor_ = other.shadowColor_ != null ? other.shadowColor_.Clone() : null;
      outlineColor_ = other.outlineColor_ != null ? other.outlineColor_.Clone() : null;
      textColor_ = other.textColor_ != null ? other.textColor_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public PBTextShape Clone() {
      return new PBTextShape(this);
    }

    /// <summary>Field number for the "text" field.</summary>
    public const int TextFieldNumber = 1;
    private string text_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Text {
      get { return text_; }
      set {
        text_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "visible" field.</summary>
    public const int VisibleFieldNumber = 2;
    private bool visible_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Visible {
      get { return visible_; }
      set {
        visible_ = value;
      }
    }

    /// <summary>Field number for the "font" field.</summary>
    public const int FontFieldNumber = 3;
    private string font_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Font {
      get { return font_; }
      set {
        font_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "opacity" field.</summary>
    public const int OpacityFieldNumber = 4;
    private float opacity_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float Opacity {
      get { return opacity_; }
      set {
        opacity_ = value;
      }
    }

    /// <summary>Field number for the "fontSize" field.</summary>
    public const int FontSizeFieldNumber = 5;
    private float fontSize_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float FontSize {
      get { return fontSize_; }
      set {
        fontSize_ = value;
      }
    }

    /// <summary>Field number for the "fontAutoSize" field.</summary>
    public const int FontAutoSizeFieldNumber = 6;
    private bool fontAutoSize_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool FontAutoSize {
      get { return fontAutoSize_; }
      set {
        fontAutoSize_ = value;
      }
    }

    /// <summary>Field number for the "hTextAlign" field.</summary>
    public const int HTextAlignFieldNumber = 7;
    private string hTextAlign_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string HTextAlign {
      get { return hTextAlign_; }
      set {
        hTextAlign_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "vTextAlign" field.</summary>
    public const int VTextAlignFieldNumber = 8;
    private string vTextAlign_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string VTextAlign {
      get { return vTextAlign_; }
      set {
        vTextAlign_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "width" field.</summary>
    public const int WidthFieldNumber = 9;
    private float width_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float Width {
      get { return width_; }
      set {
        width_ = value;
      }
    }

    /// <summary>Field number for the "height" field.</summary>
    public const int HeightFieldNumber = 10;
    private float height_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float Height {
      get { return height_; }
      set {
        height_ = value;
      }
    }

    /// <summary>Field number for the "paddingTop" field.</summary>
    public const int PaddingTopFieldNumber = 11;
    private float paddingTop_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float PaddingTop {
      get { return paddingTop_; }
      set {
        paddingTop_ = value;
      }
    }

    /// <summary>Field number for the "paddingRight" field.</summary>
    public const int PaddingRightFieldNumber = 12;
    private float paddingRight_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float PaddingRight {
      get { return paddingRight_; }
      set {
        paddingRight_ = value;
      }
    }

    /// <summary>Field number for the "paddingBottom" field.</summary>
    public const int PaddingBottomFieldNumber = 13;
    private float paddingBottom_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float PaddingBottom {
      get { return paddingBottom_; }
      set {
        paddingBottom_ = value;
      }
    }

    /// <summary>Field number for the "paddingLeft" field.</summary>
    public const int PaddingLeftFieldNumber = 14;
    private float paddingLeft_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float PaddingLeft {
      get { return paddingLeft_; }
      set {
        paddingLeft_ = value;
      }
    }

    /// <summary>Field number for the "lineSpacing" field.</summary>
    public const int LineSpacingFieldNumber = 15;
    private float lineSpacing_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float LineSpacing {
      get { return lineSpacing_; }
      set {
        lineSpacing_ = value;
      }
    }

    /// <summary>Field number for the "lineCount" field.</summary>
    public const int LineCountFieldNumber = 16;
    private int lineCount_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int LineCount {
      get { return lineCount_; }
      set {
        lineCount_ = value;
      }
    }

    /// <summary>Field number for the "textWrapping" field.</summary>
    public const int TextWrappingFieldNumber = 17;
    private bool textWrapping_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool TextWrapping {
      get { return textWrapping_; }
      set {
        textWrapping_ = value;
      }
    }

    /// <summary>Field number for the "shadowBlur" field.</summary>
    public const int ShadowBlurFieldNumber = 18;
    private float shadowBlur_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float ShadowBlur {
      get { return shadowBlur_; }
      set {
        shadowBlur_ = value;
      }
    }

    /// <summary>Field number for the "shadowOffsetX" field.</summary>
    public const int ShadowOffsetXFieldNumber = 19;
    private float shadowOffsetX_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float ShadowOffsetX {
      get { return shadowOffsetX_; }
      set {
        shadowOffsetX_ = value;
      }
    }

    /// <summary>Field number for the "shadowOffsetY" field.</summary>
    public const int ShadowOffsetYFieldNumber = 20;
    private float shadowOffsetY_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float ShadowOffsetY {
      get { return shadowOffsetY_; }
      set {
        shadowOffsetY_ = value;
      }
    }

    /// <summary>Field number for the "outlineWidth" field.</summary>
    public const int OutlineWidthFieldNumber = 21;
    private float outlineWidth_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float OutlineWidth {
      get { return outlineWidth_; }
      set {
        outlineWidth_ = value;
      }
    }

    /// <summary>Field number for the "shadowColor" field.</summary>
    public const int ShadowColorFieldNumber = 22;
    private global::DCL.ECSComponents.Color shadowColor_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::DCL.ECSComponents.Color ShadowColor {
      get { return shadowColor_; }
      set {
        shadowColor_ = value;
      }
    }

    /// <summary>Field number for the "outlineColor" field.</summary>
    public const int OutlineColorFieldNumber = 23;
    private global::DCL.ECSComponents.Color outlineColor_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::DCL.ECSComponents.Color OutlineColor {
      get { return outlineColor_; }
      set {
        outlineColor_ = value;
      }
    }

    /// <summary>Field number for the "textColor" field.</summary>
    public const int TextColorFieldNumber = 24;
    private global::DCL.ECSComponents.Color textColor_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::DCL.ECSComponents.Color TextColor {
      get { return textColor_; }
      set {
        textColor_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as PBTextShape);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(PBTextShape other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Text != other.Text) return false;
      if (Visible != other.Visible) return false;
      if (Font != other.Font) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Opacity, other.Opacity)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(FontSize, other.FontSize)) return false;
      if (FontAutoSize != other.FontAutoSize) return false;
      if (HTextAlign != other.HTextAlign) return false;
      if (VTextAlign != other.VTextAlign) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Width, other.Width)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Height, other.Height)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(PaddingTop, other.PaddingTop)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(PaddingRight, other.PaddingRight)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(PaddingBottom, other.PaddingBottom)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(PaddingLeft, other.PaddingLeft)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(LineSpacing, other.LineSpacing)) return false;
      if (LineCount != other.LineCount) return false;
      if (TextWrapping != other.TextWrapping) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(ShadowBlur, other.ShadowBlur)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(ShadowOffsetX, other.ShadowOffsetX)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(ShadowOffsetY, other.ShadowOffsetY)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(OutlineWidth, other.OutlineWidth)) return false;
      if (!object.Equals(ShadowColor, other.ShadowColor)) return false;
      if (!object.Equals(OutlineColor, other.OutlineColor)) return false;
      if (!object.Equals(TextColor, other.TextColor)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Text.Length != 0) hash ^= Text.GetHashCode();
      if (Visible != false) hash ^= Visible.GetHashCode();
      if (Font.Length != 0) hash ^= Font.GetHashCode();
      if (Opacity != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Opacity);
      if (FontSize != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(FontSize);
      if (FontAutoSize != false) hash ^= FontAutoSize.GetHashCode();
      if (HTextAlign.Length != 0) hash ^= HTextAlign.GetHashCode();
      if (VTextAlign.Length != 0) hash ^= VTextAlign.GetHashCode();
      if (Width != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Width);
      if (Height != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Height);
      if (PaddingTop != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(PaddingTop);
      if (PaddingRight != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(PaddingRight);
      if (PaddingBottom != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(PaddingBottom);
      if (PaddingLeft != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(PaddingLeft);
      if (LineSpacing != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(LineSpacing);
      if (LineCount != 0) hash ^= LineCount.GetHashCode();
      if (TextWrapping != false) hash ^= TextWrapping.GetHashCode();
      if (ShadowBlur != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(ShadowBlur);
      if (ShadowOffsetX != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(ShadowOffsetX);
      if (ShadowOffsetY != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(ShadowOffsetY);
      if (OutlineWidth != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(OutlineWidth);
      if (shadowColor_ != null) hash ^= ShadowColor.GetHashCode();
      if (outlineColor_ != null) hash ^= OutlineColor.GetHashCode();
      if (textColor_ != null) hash ^= TextColor.GetHashCode();
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
      if (Text.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Text);
      }
      if (Visible != false) {
        output.WriteRawTag(16);
        output.WriteBool(Visible);
      }
      if (Font.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(Font);
      }
      if (Opacity != 0F) {
        output.WriteRawTag(37);
        output.WriteFloat(Opacity);
      }
      if (FontSize != 0F) {
        output.WriteRawTag(45);
        output.WriteFloat(FontSize);
      }
      if (FontAutoSize != false) {
        output.WriteRawTag(48);
        output.WriteBool(FontAutoSize);
      }
      if (HTextAlign.Length != 0) {
        output.WriteRawTag(58);
        output.WriteString(HTextAlign);
      }
      if (VTextAlign.Length != 0) {
        output.WriteRawTag(66);
        output.WriteString(VTextAlign);
      }
      if (Width != 0F) {
        output.WriteRawTag(77);
        output.WriteFloat(Width);
      }
      if (Height != 0F) {
        output.WriteRawTag(85);
        output.WriteFloat(Height);
      }
      if (PaddingTop != 0F) {
        output.WriteRawTag(93);
        output.WriteFloat(PaddingTop);
      }
      if (PaddingRight != 0F) {
        output.WriteRawTag(101);
        output.WriteFloat(PaddingRight);
      }
      if (PaddingBottom != 0F) {
        output.WriteRawTag(109);
        output.WriteFloat(PaddingBottom);
      }
      if (PaddingLeft != 0F) {
        output.WriteRawTag(117);
        output.WriteFloat(PaddingLeft);
      }
      if (LineSpacing != 0F) {
        output.WriteRawTag(125);
        output.WriteFloat(LineSpacing);
      }
      if (LineCount != 0) {
        output.WriteRawTag(128, 1);
        output.WriteInt32(LineCount);
      }
      if (TextWrapping != false) {
        output.WriteRawTag(136, 1);
        output.WriteBool(TextWrapping);
      }
      if (ShadowBlur != 0F) {
        output.WriteRawTag(149, 1);
        output.WriteFloat(ShadowBlur);
      }
      if (ShadowOffsetX != 0F) {
        output.WriteRawTag(157, 1);
        output.WriteFloat(ShadowOffsetX);
      }
      if (ShadowOffsetY != 0F) {
        output.WriteRawTag(165, 1);
        output.WriteFloat(ShadowOffsetY);
      }
      if (OutlineWidth != 0F) {
        output.WriteRawTag(173, 1);
        output.WriteFloat(OutlineWidth);
      }
      if (shadowColor_ != null) {
        output.WriteRawTag(178, 1);
        output.WriteMessage(ShadowColor);
      }
      if (outlineColor_ != null) {
        output.WriteRawTag(186, 1);
        output.WriteMessage(OutlineColor);
      }
      if (textColor_ != null) {
        output.WriteRawTag(194, 1);
        output.WriteMessage(TextColor);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Text.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Text);
      }
      if (Visible != false) {
        size += 1 + 1;
      }
      if (Font.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Font);
      }
      if (Opacity != 0F) {
        size += 1 + 4;
      }
      if (FontSize != 0F) {
        size += 1 + 4;
      }
      if (FontAutoSize != false) {
        size += 1 + 1;
      }
      if (HTextAlign.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(HTextAlign);
      }
      if (VTextAlign.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(VTextAlign);
      }
      if (Width != 0F) {
        size += 1 + 4;
      }
      if (Height != 0F) {
        size += 1 + 4;
      }
      if (PaddingTop != 0F) {
        size += 1 + 4;
      }
      if (PaddingRight != 0F) {
        size += 1 + 4;
      }
      if (PaddingBottom != 0F) {
        size += 1 + 4;
      }
      if (PaddingLeft != 0F) {
        size += 1 + 4;
      }
      if (LineSpacing != 0F) {
        size += 1 + 4;
      }
      if (LineCount != 0) {
        size += 2 + pb::CodedOutputStream.ComputeInt32Size(LineCount);
      }
      if (TextWrapping != false) {
        size += 2 + 1;
      }
      if (ShadowBlur != 0F) {
        size += 2 + 4;
      }
      if (ShadowOffsetX != 0F) {
        size += 2 + 4;
      }
      if (ShadowOffsetY != 0F) {
        size += 2 + 4;
      }
      if (OutlineWidth != 0F) {
        size += 2 + 4;
      }
      if (shadowColor_ != null) {
        size += 2 + pb::CodedOutputStream.ComputeMessageSize(ShadowColor);
      }
      if (outlineColor_ != null) {
        size += 2 + pb::CodedOutputStream.ComputeMessageSize(OutlineColor);
      }
      if (textColor_ != null) {
        size += 2 + pb::CodedOutputStream.ComputeMessageSize(TextColor);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(PBTextShape other) {
      if (other == null) {
        return;
      }
      if (other.Text.Length != 0) {
        Text = other.Text;
      }
      if (other.Visible != false) {
        Visible = other.Visible;
      }
      if (other.Font.Length != 0) {
        Font = other.Font;
      }
      if (other.Opacity != 0F) {
        Opacity = other.Opacity;
      }
      if (other.FontSize != 0F) {
        FontSize = other.FontSize;
      }
      if (other.FontAutoSize != false) {
        FontAutoSize = other.FontAutoSize;
      }
      if (other.HTextAlign.Length != 0) {
        HTextAlign = other.HTextAlign;
      }
      if (other.VTextAlign.Length != 0) {
        VTextAlign = other.VTextAlign;
      }
      if (other.Width != 0F) {
        Width = other.Width;
      }
      if (other.Height != 0F) {
        Height = other.Height;
      }
      if (other.PaddingTop != 0F) {
        PaddingTop = other.PaddingTop;
      }
      if (other.PaddingRight != 0F) {
        PaddingRight = other.PaddingRight;
      }
      if (other.PaddingBottom != 0F) {
        PaddingBottom = other.PaddingBottom;
      }
      if (other.PaddingLeft != 0F) {
        PaddingLeft = other.PaddingLeft;
      }
      if (other.LineSpacing != 0F) {
        LineSpacing = other.LineSpacing;
      }
      if (other.LineCount != 0) {
        LineCount = other.LineCount;
      }
      if (other.TextWrapping != false) {
        TextWrapping = other.TextWrapping;
      }
      if (other.ShadowBlur != 0F) {
        ShadowBlur = other.ShadowBlur;
      }
      if (other.ShadowOffsetX != 0F) {
        ShadowOffsetX = other.ShadowOffsetX;
      }
      if (other.ShadowOffsetY != 0F) {
        ShadowOffsetY = other.ShadowOffsetY;
      }
      if (other.OutlineWidth != 0F) {
        OutlineWidth = other.OutlineWidth;
      }
      if (other.shadowColor_ != null) {
        if (shadowColor_ == null) {
          ShadowColor = new global::DCL.ECSComponents.Color();
        }
        ShadowColor.MergeFrom(other.ShadowColor);
      }
      if (other.outlineColor_ != null) {
        if (outlineColor_ == null) {
          OutlineColor = new global::DCL.ECSComponents.Color();
        }
        OutlineColor.MergeFrom(other.OutlineColor);
      }
      if (other.textColor_ != null) {
        if (textColor_ == null) {
          TextColor = new global::DCL.ECSComponents.Color();
        }
        TextColor.MergeFrom(other.TextColor);
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
          case 10: {
            Text = input.ReadString();
            break;
          }
          case 16: {
            Visible = input.ReadBool();
            break;
          }
          case 26: {
            Font = input.ReadString();
            break;
          }
          case 37: {
            Opacity = input.ReadFloat();
            break;
          }
          case 45: {
            FontSize = input.ReadFloat();
            break;
          }
          case 48: {
            FontAutoSize = input.ReadBool();
            break;
          }
          case 58: {
            HTextAlign = input.ReadString();
            break;
          }
          case 66: {
            VTextAlign = input.ReadString();
            break;
          }
          case 77: {
            Width = input.ReadFloat();
            break;
          }
          case 85: {
            Height = input.ReadFloat();
            break;
          }
          case 93: {
            PaddingTop = input.ReadFloat();
            break;
          }
          case 101: {
            PaddingRight = input.ReadFloat();
            break;
          }
          case 109: {
            PaddingBottom = input.ReadFloat();
            break;
          }
          case 117: {
            PaddingLeft = input.ReadFloat();
            break;
          }
          case 125: {
            LineSpacing = input.ReadFloat();
            break;
          }
          case 128: {
            LineCount = input.ReadInt32();
            break;
          }
          case 136: {
            TextWrapping = input.ReadBool();
            break;
          }
          case 149: {
            ShadowBlur = input.ReadFloat();
            break;
          }
          case 157: {
            ShadowOffsetX = input.ReadFloat();
            break;
          }
          case 165: {
            ShadowOffsetY = input.ReadFloat();
            break;
          }
          case 173: {
            OutlineWidth = input.ReadFloat();
            break;
          }
          case 178: {
            if (shadowColor_ == null) {
              ShadowColor = new global::DCL.ECSComponents.Color();
            }
            input.ReadMessage(ShadowColor);
            break;
          }
          case 186: {
            if (outlineColor_ == null) {
              OutlineColor = new global::DCL.ECSComponents.Color();
            }
            input.ReadMessage(OutlineColor);
            break;
          }
          case 194: {
            if (textColor_ == null) {
              TextColor = new global::DCL.ECSComponents.Color();
            }
            input.ReadMessage(TextColor);
            break;
          }
        }
      }
    }

  }

  public sealed partial class Color : pb::IMessage<Color> {
    private static readonly pb::MessageParser<Color> _parser = new pb::MessageParser<Color>(() => new Color());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Color> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::DCL.ECSComponents.TextShapeReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Color() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Color(Color other) : this() {
      red_ = other.red_;
      green_ = other.green_;
      blue_ = other.blue_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Color Clone() {
      return new Color(this);
    }

    /// <summary>Field number for the "red" field.</summary>
    public const int RedFieldNumber = 1;
    private float red_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float Red {
      get { return red_; }
      set {
        red_ = value;
      }
    }

    /// <summary>Field number for the "green" field.</summary>
    public const int GreenFieldNumber = 2;
    private float green_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float Green {
      get { return green_; }
      set {
        green_ = value;
      }
    }

    /// <summary>Field number for the "blue" field.</summary>
    public const int BlueFieldNumber = 3;
    private float blue_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float Blue {
      get { return blue_; }
      set {
        blue_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Color);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Color other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Red, other.Red)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Green, other.Green)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Blue, other.Blue)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Red != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Red);
      if (Green != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Green);
      if (Blue != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Blue);
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
      if (Red != 0F) {
        output.WriteRawTag(13);
        output.WriteFloat(Red);
      }
      if (Green != 0F) {
        output.WriteRawTag(21);
        output.WriteFloat(Green);
      }
      if (Blue != 0F) {
        output.WriteRawTag(29);
        output.WriteFloat(Blue);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Red != 0F) {
        size += 1 + 4;
      }
      if (Green != 0F) {
        size += 1 + 4;
      }
      if (Blue != 0F) {
        size += 1 + 4;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Color other) {
      if (other == null) {
        return;
      }
      if (other.Red != 0F) {
        Red = other.Red;
      }
      if (other.Green != 0F) {
        Green = other.Green;
      }
      if (other.Blue != 0F) {
        Blue = other.Blue;
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
            Red = input.ReadFloat();
            break;
          }
          case 21: {
            Green = input.ReadFloat();
            break;
          }
          case 29: {
            Blue = input.ReadFloat();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
