// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: OnPointerResult.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace DCL.ECSComponents {

  /// <summary>Holder for reflection information generated from OnPointerResult.proto</summary>
  public static partial class OnPointerResultReflection {

    #region Descriptor
    /// <summary>File descriptor for OnPointerResult.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static OnPointerResultReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChVPblBvaW50ZXJSZXN1bHQucHJvdG8SEGRlY2VudHJhbGFuZC5lY3MaEFJh",
            "eWNhc3RIaXQucHJvdG8aGWNvbW1vbi9BY3Rpb25CdXR0b24ucHJvdG8inQIK",
            "FVBCUG9pbnRlckV2ZW50c1Jlc3VsdBJICghjb21tYW5kcxgBIAMoCzI2LmRl",
            "Y2VudHJhbGFuZC5lY3MuUEJQb2ludGVyRXZlbnRzUmVzdWx0LlBvaW50ZXJD",
            "b21tYW5kGrkBCg5Qb2ludGVyQ29tbWFuZBIdCgZidXR0b24YASABKA4yDS5B",
            "Y3Rpb25CdXR0b24SKQoDaGl0GAIgASgLMhwuZGVjZW50cmFsYW5kLmVjcy5S",
            "YXljYXN0SGl0EioKBXN0YXRlGAQgASgOMhsuZGVjZW50cmFsYW5kLmVjcy5T",
            "dGF0ZUVudW0SEQoJdGltZXN0YW1wGAUgASgFEhMKBmFuYWxvZxgGIAEoAkgA",
            "iAEBQgkKB19hbmFsb2cqfQoJU3RhdGVFbnVtEhAKDFN0YXRlRW51bV9VUBAA",
            "EhIKDlN0YXRlRW51bV9ET1dOEAESFAoQU3RhdGVFbnVtX0FOQUxPRxACEhkK",
            "FVN0YXRlRW51bV9IT1ZFUl9FTlRFUhADEhkKFVN0YXRlRW51bV9IT1ZFUl9M",
            "RUFWRRAEQhSqAhFEQ0wuRUNTQ29tcG9uZW50c2IGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::DCL.ECSComponents.RaycastHitReflection.Descriptor, global::ActionButtonReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::DCL.ECSComponents.StateEnum), }, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::DCL.ECSComponents.PBPointerEventsResult), global::DCL.ECSComponents.PBPointerEventsResult.Parser, new[]{ "Commands" }, null, null, null, new pbr::GeneratedClrTypeInfo[] { new pbr::GeneratedClrTypeInfo(typeof(global::DCL.ECSComponents.PBPointerEventsResult.Types.PointerCommand), global::DCL.ECSComponents.PBPointerEventsResult.Types.PointerCommand.Parser, new[]{ "Button", "Hit", "State", "Timestamp", "Analog" }, new[]{ "Analog" }, null, null, null)})
          }));
    }
    #endregion

  }
  #region Enums
  public enum StateEnum {
    [pbr::OriginalName("StateEnum_UP")] Up = 0,
    [pbr::OriginalName("StateEnum_DOWN")] Down = 1,
    [pbr::OriginalName("StateEnum_ANALOG")] Analog = 2,
    [pbr::OriginalName("StateEnum_HOVER_ENTER")] HoverEnter = 3,
    [pbr::OriginalName("StateEnum_HOVER_LEAVE")] HoverLeave = 4,
  }

  #endregion

  #region Messages
  /// <summary>
  /// the renderer will set this component to the entity after every pointer event
  /// </summary>
  public sealed partial class PBPointerEventsResult : pb::IMessage<PBPointerEventsResult>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<PBPointerEventsResult> _parser = new pb::MessageParser<PBPointerEventsResult>(() => new PBPointerEventsResult());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<PBPointerEventsResult> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::DCL.ECSComponents.OnPointerResultReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBPointerEventsResult() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBPointerEventsResult(PBPointerEventsResult other) : this() {
      commands_ = other.commands_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBPointerEventsResult Clone() {
      return new PBPointerEventsResult(this);
    }

    /// <summary>Field number for the "commands" field.</summary>
    public const int CommandsFieldNumber = 1;
    private static readonly pb::FieldCodec<global::DCL.ECSComponents.PBPointerEventsResult.Types.PointerCommand> _repeated_commands_codec
        = pb::FieldCodec.ForMessage(10, global::DCL.ECSComponents.PBPointerEventsResult.Types.PointerCommand.Parser);
    private readonly pbc::RepeatedField<global::DCL.ECSComponents.PBPointerEventsResult.Types.PointerCommand> commands_ = new pbc::RepeatedField<global::DCL.ECSComponents.PBPointerEventsResult.Types.PointerCommand>();
    /// <summary>
    /// a list of the last N pointer commands (from the engine)
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<global::DCL.ECSComponents.PBPointerEventsResult.Types.PointerCommand> Commands {
      get { return commands_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as PBPointerEventsResult);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(PBPointerEventsResult other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!commands_.Equals(other.commands_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= commands_.GetHashCode();
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
      commands_.WriteTo(output, _repeated_commands_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      commands_.WriteTo(ref output, _repeated_commands_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      size += commands_.CalculateSize(_repeated_commands_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(PBPointerEventsResult other) {
      if (other == null) {
        return;
      }
      commands_.Add(other.commands_);
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
            commands_.AddEntriesFrom(input, _repeated_commands_codec);
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
            commands_.AddEntriesFrom(ref input, _repeated_commands_codec);
            break;
          }
        }
      }
    }
    #endif

    #region Nested types
    /// <summary>Container for nested types declared in the PBPointerEventsResult message type.</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static partial class Types {
      /// <summary>
      /// this message represents a raycast, used both for UP and DOWN actions
      /// </summary>
      public sealed partial class PointerCommand : pb::IMessage<PointerCommand>
      #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
          , pb::IBufferMessage
      #endif
      {
        private static readonly pb::MessageParser<PointerCommand> _parser = new pb::MessageParser<PointerCommand>(() => new PointerCommand());
        private pb::UnknownFieldSet _unknownFields;
        private int _hasBits0;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public static pb::MessageParser<PointerCommand> Parser { get { return _parser; } }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public static pbr::MessageDescriptor Descriptor {
          get { return global::DCL.ECSComponents.PBPointerEventsResult.Descriptor.NestedTypes[0]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        pbr::MessageDescriptor pb::IMessage.Descriptor {
          get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public PointerCommand() {
          OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public PointerCommand(PointerCommand other) : this() {
          _hasBits0 = other._hasBits0;
          button_ = other.button_;
          hit_ = other.hit_ != null ? other.hit_.Clone() : null;
          state_ = other.state_;
          timestamp_ = other.timestamp_;
          analog_ = other.analog_;
          _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public PointerCommand Clone() {
          return new PointerCommand(this);
        }

        /// <summary>Field number for the "button" field.</summary>
        public const int ButtonFieldNumber = 1;
        private global::ActionButton button_ = global::ActionButton.Pointer;
        /// <summary>
        /// identifier of the input
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public global::ActionButton Button {
          get { return button_; }
          set {
            button_ = value;
          }
        }

        /// <summary>Field number for the "hit" field.</summary>
        public const int HitFieldNumber = 2;
        private global::DCL.ECSComponents.RaycastHit hit_;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public global::DCL.ECSComponents.RaycastHit Hit {
          get { return hit_; }
          set {
            hit_ = value;
          }
        }

        /// <summary>Field number for the "state" field.</summary>
        public const int StateFieldNumber = 4;
        private global::DCL.ECSComponents.StateEnum state_ = global::DCL.ECSComponents.StateEnum.Up;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public global::DCL.ECSComponents.StateEnum State {
          get { return state_; }
          set {
            state_ = value;
          }
        }

        /// <summary>Field number for the "timestamp" field.</summary>
        public const int TimestampFieldNumber = 5;
        private int timestamp_;
        /// <summary>
        /// could be a Lamport timestamp
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public int Timestamp {
          get { return timestamp_; }
          set {
            timestamp_ = value;
          }
        }

        /// <summary>Field number for the "analog" field.</summary>
        public const int AnalogFieldNumber = 6;
        private float analog_;
        /// <summary>
        /// if the input is analog then we store it here
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public float Analog {
          get { if ((_hasBits0 & 1) != 0) { return analog_; } else { return 0F; } }
          set {
            _hasBits0 |= 1;
            analog_ = value;
          }
        }
        /// <summary>Gets whether the "analog" field is set</summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public bool HasAnalog {
          get { return (_hasBits0 & 1) != 0; }
        }
        /// <summary>Clears the value of the "analog" field</summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public void ClearAnalog() {
          _hasBits0 &= ~1;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public override bool Equals(object other) {
          return Equals(other as PointerCommand);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public bool Equals(PointerCommand other) {
          if (ReferenceEquals(other, null)) {
            return false;
          }
          if (ReferenceEquals(other, this)) {
            return true;
          }
          if (Button != other.Button) return false;
          if (!object.Equals(Hit, other.Hit)) return false;
          if (State != other.State) return false;
          if (Timestamp != other.Timestamp) return false;
          if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Analog, other.Analog)) return false;
          return Equals(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public override int GetHashCode() {
          int hash = 1;
          if (Button != global::ActionButton.Pointer) hash ^= Button.GetHashCode();
          if (hit_ != null) hash ^= Hit.GetHashCode();
          if (State != global::DCL.ECSComponents.StateEnum.Up) hash ^= State.GetHashCode();
          if (Timestamp != 0) hash ^= Timestamp.GetHashCode();
          if (HasAnalog) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Analog);
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
          if (Button != global::ActionButton.Pointer) {
            output.WriteRawTag(8);
            output.WriteEnum((int) Button);
          }
          if (hit_ != null) {
            output.WriteRawTag(18);
            output.WriteMessage(Hit);
          }
          if (State != global::DCL.ECSComponents.StateEnum.Up) {
            output.WriteRawTag(32);
            output.WriteEnum((int) State);
          }
          if (Timestamp != 0) {
            output.WriteRawTag(40);
            output.WriteInt32(Timestamp);
          }
          if (HasAnalog) {
            output.WriteRawTag(53);
            output.WriteFloat(Analog);
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
          if (Button != global::ActionButton.Pointer) {
            output.WriteRawTag(8);
            output.WriteEnum((int) Button);
          }
          if (hit_ != null) {
            output.WriteRawTag(18);
            output.WriteMessage(Hit);
          }
          if (State != global::DCL.ECSComponents.StateEnum.Up) {
            output.WriteRawTag(32);
            output.WriteEnum((int) State);
          }
          if (Timestamp != 0) {
            output.WriteRawTag(40);
            output.WriteInt32(Timestamp);
          }
          if (HasAnalog) {
            output.WriteRawTag(53);
            output.WriteFloat(Analog);
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
          if (Button != global::ActionButton.Pointer) {
            size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Button);
          }
          if (hit_ != null) {
            size += 1 + pb::CodedOutputStream.ComputeMessageSize(Hit);
          }
          if (State != global::DCL.ECSComponents.StateEnum.Up) {
            size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) State);
          }
          if (Timestamp != 0) {
            size += 1 + pb::CodedOutputStream.ComputeInt32Size(Timestamp);
          }
          if (HasAnalog) {
            size += 1 + 4;
          }
          if (_unknownFields != null) {
            size += _unknownFields.CalculateSize();
          }
          return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public void MergeFrom(PointerCommand other) {
          if (other == null) {
            return;
          }
          if (other.Button != global::ActionButton.Pointer) {
            Button = other.Button;
          }
          if (other.hit_ != null) {
            if (hit_ == null) {
              Hit = new global::DCL.ECSComponents.RaycastHit();
            }
            Hit.MergeFrom(other.Hit);
          }
          if (other.State != global::DCL.ECSComponents.StateEnum.Up) {
            State = other.State;
          }
          if (other.Timestamp != 0) {
            Timestamp = other.Timestamp;
          }
          if (other.HasAnalog) {
            Analog = other.Analog;
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
                if (hit_ == null) {
                  Hit = new global::DCL.ECSComponents.RaycastHit();
                }
                input.ReadMessage(Hit);
                break;
              }
              case 32: {
                State = (global::DCL.ECSComponents.StateEnum) input.ReadEnum();
                break;
              }
              case 40: {
                Timestamp = input.ReadInt32();
                break;
              }
              case 53: {
                Analog = input.ReadFloat();
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
                if (hit_ == null) {
                  Hit = new global::DCL.ECSComponents.RaycastHit();
                }
                input.ReadMessage(Hit);
                break;
              }
              case 32: {
                State = (global::DCL.ECSComponents.StateEnum) input.ReadEnum();
                break;
              }
              case 40: {
                Timestamp = input.ReadInt32();
                break;
              }
              case 53: {
                Analog = input.ReadFloat();
                break;
              }
            }
          }
        }
        #endif

      }

    }
    #endregion

  }

  #endregion

}

#endregion Designer generated code
