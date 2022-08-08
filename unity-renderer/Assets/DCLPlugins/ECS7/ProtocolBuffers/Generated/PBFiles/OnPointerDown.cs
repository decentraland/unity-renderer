// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: OnPointerDown.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace DCL.ECSComponents {

  /// <summary>Holder for reflection information generated from OnPointerDown.proto</summary>
  public static partial class OnPointerDownReflection {

    #region Descriptor
    /// <summary>File descriptor for OnPointerDown.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static OnPointerDownReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChNPblBvaW50ZXJEb3duLnByb3RvEhBkZWNlbnRyYWxhbmQuZWNzGhljb21t",
            "b24vQWN0aW9uQnV0dG9uLnByb3RvIroBCg9QQk9uUG9pbnRlckRvd24SIgoG",
            "YnV0dG9uGAEgASgOMg0uQWN0aW9uQnV0dG9uSACIAQESFwoKaG92ZXJfdGV4",
            "dBgCIAEoCUgBiAEBEhUKCGRpc3RhbmNlGAMgASgCSAKIAQESGgoNc2hvd19m",
            "ZWVkYmFjaxgEIAEoCEgDiAEBQgkKB19idXR0b25CDQoLX2hvdmVyX3RleHRC",
            "CwoJX2Rpc3RhbmNlQhAKDl9zaG93X2ZlZWRiYWNrQhSqAhFEQ0wuRUNTQ29t",
            "cG9uZW50c2IGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::ActionButtonReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::DCL.ECSComponents.PBOnPointerDown), global::DCL.ECSComponents.PBOnPointerDown.Parser, new[]{ "Button", "HoverText", "Distance", "ShowFeedback" }, new[]{ "Button", "HoverText", "Distance", "ShowFeedback" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class PBOnPointerDown : pb::IMessage<PBOnPointerDown>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<PBOnPointerDown> _parser = new pb::MessageParser<PBOnPointerDown>(() => new PBOnPointerDown());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<PBOnPointerDown> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::DCL.ECSComponents.OnPointerDownReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBOnPointerDown() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBOnPointerDown(PBOnPointerDown other) : this() {
      _hasBits0 = other._hasBits0;
      button_ = other.button_;
      hoverText_ = other.hoverText_;
      distance_ = other.distance_;
      showFeedback_ = other.showFeedback_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBOnPointerDown Clone() {
      return new PBOnPointerDown(this);
    }

    /// <summary>Field number for the "button" field.</summary>
    public const int ButtonFieldNumber = 1;
    private global::ActionButton button_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::ActionButton Button {
      get { if ((_hasBits0 & 1) != 0) { return button_; } else { return global::ActionButton.Pointer; } }
      set {
        _hasBits0 |= 1;
        button_ = value;
      }
    }
    /// <summary>Gets whether the "button" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasButton {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "button" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearButton() {
      _hasBits0 &= ~1;
    }

    /// <summary>Field number for the "hover_text" field.</summary>
    public const int HoverTextFieldNumber = 2;
    private string hoverText_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string HoverText {
      get { return hoverText_ ?? ""; }
      set {
        hoverText_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }
    /// <summary>Gets whether the "hover_text" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasHoverText {
      get { return hoverText_ != null; }
    }
    /// <summary>Clears the value of the "hover_text" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearHoverText() {
      hoverText_ = null;
    }

    /// <summary>Field number for the "distance" field.</summary>
    public const int DistanceFieldNumber = 3;
    private float distance_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Distance {
      get { if ((_hasBits0 & 2) != 0) { return distance_; } else { return 0F; } }
      set {
        _hasBits0 |= 2;
        distance_ = value;
      }
    }
    /// <summary>Gets whether the "distance" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasDistance {
      get { return (_hasBits0 & 2) != 0; }
    }
    /// <summary>Clears the value of the "distance" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearDistance() {
      _hasBits0 &= ~2;
    }

    /// <summary>Field number for the "show_feedback" field.</summary>
    public const int ShowFeedbackFieldNumber = 4;
    private bool showFeedback_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool ShowFeedback {
      get { if ((_hasBits0 & 4) != 0) { return showFeedback_; } else { return false; } }
      set {
        _hasBits0 |= 4;
        showFeedback_ = value;
      }
    }
    /// <summary>Gets whether the "show_feedback" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasShowFeedback {
      get { return (_hasBits0 & 4) != 0; }
    }
    /// <summary>Clears the value of the "show_feedback" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearShowFeedback() {
      _hasBits0 &= ~4;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as PBOnPointerDown);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(PBOnPointerDown other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Button != other.Button) return false;
      if (HoverText != other.HoverText) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Distance, other.Distance)) return false;
      if (ShowFeedback != other.ShowFeedback) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (HasButton) hash ^= Button.GetHashCode();
      if (HasHoverText) hash ^= HoverText.GetHashCode();
      if (HasDistance) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Distance);
      if (HasShowFeedback) hash ^= ShowFeedback.GetHashCode();
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
      if (HasButton) {
        output.WriteRawTag(8);
        output.WriteEnum((int) Button);
      }
      if (HasHoverText) {
        output.WriteRawTag(18);
        output.WriteString(HoverText);
      }
      if (HasDistance) {
        output.WriteRawTag(29);
        output.WriteFloat(Distance);
      }
      if (HasShowFeedback) {
        output.WriteRawTag(32);
        output.WriteBool(ShowFeedback);
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
      if (HasButton) {
        output.WriteRawTag(8);
        output.WriteEnum((int) Button);
      }
      if (HasHoverText) {
        output.WriteRawTag(18);
        output.WriteString(HoverText);
      }
      if (HasDistance) {
        output.WriteRawTag(29);
        output.WriteFloat(Distance);
      }
      if (HasShowFeedback) {
        output.WriteRawTag(32);
        output.WriteBool(ShowFeedback);
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
      if (HasButton) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Button);
      }
      if (HasHoverText) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(HoverText);
      }
      if (HasDistance) {
        size += 1 + 4;
      }
      if (HasShowFeedback) {
        size += 1 + 1;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(PBOnPointerDown other) {
      if (other == null) {
        return;
      }
      if (other.HasButton) {
        Button = other.Button;
      }
      if (other.HasHoverText) {
        HoverText = other.HoverText;
      }
      if (other.HasDistance) {
        Distance = other.Distance;
      }
      if (other.HasShowFeedback) {
        ShowFeedback = other.ShowFeedback;
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
            Button = (global::ActionButton) input.ReadEnum();
            break;
          }
          case 18: {
            HoverText = input.ReadString();
            break;
          }
          case 29: {
            Distance = input.ReadFloat();
            break;
          }
          case 32: {
            ShowFeedback = input.ReadBool();
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
            Button = (global::ActionButton) input.ReadEnum();
            break;
          }
          case 18: {
            HoverText = input.ReadString();
            break;
          }
          case 29: {
            Distance = input.ReadFloat();
            break;
          }
          case 32: {
            ShowFeedback = input.ReadBool();
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
