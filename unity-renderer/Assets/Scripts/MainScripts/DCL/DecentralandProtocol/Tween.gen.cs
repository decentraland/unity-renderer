// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: decentraland/sdk/components/tween.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace DCL.ECSComponents {

  /// <summary>Holder for reflection information generated from decentraland/sdk/components/tween.proto</summary>
  public static partial class TweenReflection {

    #region Descriptor
    /// <summary>File descriptor for decentraland/sdk/components/tween.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static TweenReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CidkZWNlbnRyYWxhbmQvc2RrL2NvbXBvbmVudHMvdHdlZW4ucHJvdG8SG2Rl",
            "Y2VudHJhbGFuZC5zZGsuY29tcG9uZW50cxohZGVjZW50cmFsYW5kL2NvbW1v",
            "bi92ZWN0b3JzLnByb3RvIrMBCgdQQlR3ZWVuEhAKCGR1cmF0aW9uGAEgASgC",
            "EkMKDnR3ZWVuX2Z1bmN0aW9uGAIgASgOMisuZGVjZW50cmFsYW5kLnNkay5j",
            "b21wb25lbnRzLkVhc2luZ0Z1bmN0aW9uEi8KBG1vdmUYAyABKAsyIS5kZWNl",
            "bnRyYWxhbmQuc2RrLmNvbXBvbmVudHMuTW92ZRIUCgdwbGF5aW5nGAQgASgI",
            "SACIAQFCCgoIX3BsYXlpbmciXgoETW92ZRIrCgVzdGFydBgBIAEoCzIcLmRl",
            "Y2VudHJhbGFuZC5jb21tb24uVmVjdG9yMxIpCgNlbmQYAiABKAsyHC5kZWNl",
            "bnRyYWxhbmQuY29tbW9uLlZlY3RvcjMqHwoORWFzaW5nRnVuY3Rpb24SDQoJ",
            "VEZfTElORUFSEABCFKoCEURDTC5FQ1NDb21wb25lbnRzYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Decentraland.Common.VectorsReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::DCL.ECSComponents.EasingFunction), }, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::DCL.ECSComponents.PBTween), global::DCL.ECSComponents.PBTween.Parser, new[]{ "Duration", "TweenFunction", "Move", "Playing" }, new[]{ "Playing" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::DCL.ECSComponents.Move), global::DCL.ECSComponents.Move.Parser, new[]{ "Start", "End" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Enums
  public enum EasingFunction {
    /// <summary>
    /// default
    /// </summary>
    [pbr::OriginalName("TF_LINEAR")] TfLinear = 0,
  }

  #endregion

  #region Messages
  public sealed partial class PBTween : pb::IMessage<PBTween>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<PBTween> _parser = new pb::MessageParser<PBTween>(() => new PBTween());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<PBTween> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::DCL.ECSComponents.TweenReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBTween() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBTween(PBTween other) : this() {
      _hasBits0 = other._hasBits0;
      duration_ = other.duration_;
      tweenFunction_ = other.tweenFunction_;
      move_ = other.move_ != null ? other.move_.Clone() : null;
      playing_ = other.playing_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBTween Clone() {
      return new PBTween(this);
    }

    /// <summary>Field number for the "duration" field.</summary>
    public const int DurationFieldNumber = 1;
    private float duration_;
    /// <summary>
    /// in milliseconds
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Duration {
      get { return duration_; }
      set {
        duration_ = value;
      }
    }

    /// <summary>Field number for the "tween_function" field.</summary>
    public const int TweenFunctionFieldNumber = 2;
    private global::DCL.ECSComponents.EasingFunction tweenFunction_ = global::DCL.ECSComponents.EasingFunction.TfLinear;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::DCL.ECSComponents.EasingFunction TweenFunction {
      get { return tweenFunction_; }
      set {
        tweenFunction_ = value;
      }
    }

    /// <summary>Field number for the "move" field.</summary>
    public const int MoveFieldNumber = 3;
    private global::DCL.ECSComponents.Move move_;
    /// <summary>
    /// TBD: oneof scale / rotation / position
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::DCL.ECSComponents.Move Move {
      get { return move_; }
      set {
        move_ = value;
      }
    }

    /// <summary>Field number for the "playing" field.</summary>
    public const int PlayingFieldNumber = 4;
    private bool playing_;
    /// <summary>
    /// default true (pause or running)
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Playing {
      get { if ((_hasBits0 & 1) != 0) { return playing_; } else { return false; } }
      set {
        _hasBits0 |= 1;
        playing_ = value;
      }
    }
    /// <summary>Gets whether the "playing" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasPlaying {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "playing" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearPlaying() {
      _hasBits0 &= ~1;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as PBTween);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(PBTween other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Duration, other.Duration)) return false;
      if (TweenFunction != other.TweenFunction) return false;
      if (!object.Equals(Move, other.Move)) return false;
      if (Playing != other.Playing) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (Duration != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Duration);
      if (TweenFunction != global::DCL.ECSComponents.EasingFunction.TfLinear) hash ^= TweenFunction.GetHashCode();
      if (move_ != null) hash ^= Move.GetHashCode();
      if (HasPlaying) hash ^= Playing.GetHashCode();
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
      if (Duration != 0F) {
        output.WriteRawTag(13);
        output.WriteFloat(Duration);
      }
      if (TweenFunction != global::DCL.ECSComponents.EasingFunction.TfLinear) {
        output.WriteRawTag(16);
        output.WriteEnum((int) TweenFunction);
      }
      if (move_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(Move);
      }
      if (HasPlaying) {
        output.WriteRawTag(32);
        output.WriteBool(Playing);
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
      if (Duration != 0F) {
        output.WriteRawTag(13);
        output.WriteFloat(Duration);
      }
      if (TweenFunction != global::DCL.ECSComponents.EasingFunction.TfLinear) {
        output.WriteRawTag(16);
        output.WriteEnum((int) TweenFunction);
      }
      if (move_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(Move);
      }
      if (HasPlaying) {
        output.WriteRawTag(32);
        output.WriteBool(Playing);
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
      if (Duration != 0F) {
        size += 1 + 4;
      }
      if (TweenFunction != global::DCL.ECSComponents.EasingFunction.TfLinear) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) TweenFunction);
      }
      if (move_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Move);
      }
      if (HasPlaying) {
        size += 1 + 1;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(PBTween other) {
      if (other == null) {
        return;
      }
      if (other.Duration != 0F) {
        Duration = other.Duration;
      }
      if (other.TweenFunction != global::DCL.ECSComponents.EasingFunction.TfLinear) {
        TweenFunction = other.TweenFunction;
      }
      if (other.move_ != null) {
        if (move_ == null) {
          Move = new global::DCL.ECSComponents.Move();
        }
        Move.MergeFrom(other.Move);
      }
      if (other.HasPlaying) {
        Playing = other.Playing;
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
          case 13: {
            Duration = input.ReadFloat();
            break;
          }
          case 16: {
            TweenFunction = (global::DCL.ECSComponents.EasingFunction) input.ReadEnum();
            break;
          }
          case 26: {
            if (move_ == null) {
              Move = new global::DCL.ECSComponents.Move();
            }
            input.ReadMessage(Move);
            break;
          }
          case 32: {
            Playing = input.ReadBool();
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
          case 13: {
            Duration = input.ReadFloat();
            break;
          }
          case 16: {
            TweenFunction = (global::DCL.ECSComponents.EasingFunction) input.ReadEnum();
            break;
          }
          case 26: {
            if (move_ == null) {
              Move = new global::DCL.ECSComponents.Move();
            }
            input.ReadMessage(Move);
            break;
          }
          case 32: {
            Playing = input.ReadBool();
            break;
          }
        }
      }
    }
    #endif

  }

  public sealed partial class Move : pb::IMessage<Move>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<Move> _parser = new pb::MessageParser<Move>(() => new Move());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<Move> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::DCL.ECSComponents.TweenReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public Move() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public Move(Move other) : this() {
      start_ = other.start_ != null ? other.start_.Clone() : null;
      end_ = other.end_ != null ? other.end_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public Move Clone() {
      return new Move(this);
    }

    /// <summary>Field number for the "start" field.</summary>
    public const int StartFieldNumber = 1;
    private global::Decentraland.Common.Vector3 start_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Decentraland.Common.Vector3 Start {
      get { return start_; }
      set {
        start_ = value;
      }
    }

    /// <summary>Field number for the "end" field.</summary>
    public const int EndFieldNumber = 2;
    private global::Decentraland.Common.Vector3 end_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Decentraland.Common.Vector3 End {
      get { return end_; }
      set {
        end_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as Move);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(Move other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Start, other.Start)) return false;
      if (!object.Equals(End, other.End)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (start_ != null) hash ^= Start.GetHashCode();
      if (end_ != null) hash ^= End.GetHashCode();
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
      if (start_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Start);
      }
      if (end_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(End);
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
      if (start_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Start);
      }
      if (end_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(End);
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
      if (start_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Start);
      }
      if (end_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(End);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(Move other) {
      if (other == null) {
        return;
      }
      if (other.start_ != null) {
        if (start_ == null) {
          Start = new global::Decentraland.Common.Vector3();
        }
        Start.MergeFrom(other.Start);
      }
      if (other.end_ != null) {
        if (end_ == null) {
          End = new global::Decentraland.Common.Vector3();
        }
        End.MergeFrom(other.End);
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
            if (start_ == null) {
              Start = new global::Decentraland.Common.Vector3();
            }
            input.ReadMessage(Start);
            break;
          }
          case 18: {
            if (end_ == null) {
              End = new global::Decentraland.Common.Vector3();
            }
            input.ReadMessage(End);
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
            if (start_ == null) {
              Start = new global::Decentraland.Common.Vector3();
            }
            input.ReadMessage(Start);
            break;
          }
          case 18: {
            if (end_ == null) {
              End = new global::Decentraland.Common.Vector3();
            }
            input.ReadMessage(End);
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