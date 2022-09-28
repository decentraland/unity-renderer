// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: AudioStream.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
/// <summary>Holder for reflection information generated from AudioStream.proto</summary>
public static partial class AudioStreamReflection {

  #region Descriptor
  /// <summary>File descriptor for AudioStream.proto</summary>
  public static pbr::FileDescriptor Descriptor {
    get { return descriptor; }
  }
  private static pbr::FileDescriptor descriptor;

  static AudioStreamReflection() {
    byte[] descriptorData = global::System.Convert.FromBase64String(
        string.Concat(
          "ChFBdWRpb1N0cmVhbS5wcm90bxoPY29tbW9uL2lkLnByb3RvIl4KDVBCQXVk",
          "aW9TdHJlYW0SFAoHcGxheWluZxgBIAEoCEgAiAEBEhMKBnZvbHVtZRgCIAEo",
          "AkgBiAEBEgsKA3VybBgDIAEoCUIKCghfcGxheWluZ0IJCgdfdm9sdW1lQgWA",
          "tRj9B2IGcHJvdG8z"));
    descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
        new pbr::FileDescriptor[] { global::IdReflection.Descriptor, },
        new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
          new pbr::GeneratedClrTypeInfo(typeof(global::PBAudioStream), global::PBAudioStream.Parser, new[]{ "Playing", "Volume", "Url" }, new[]{ "Playing", "Volume" }, null, null, null)
        }));
  }
  #endregion

}
#region Messages
public sealed partial class PBAudioStream : pb::IMessage<PBAudioStream>
#if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    , pb::IBufferMessage
#endif
{
  private static readonly pb::MessageParser<PBAudioStream> _parser = new pb::MessageParser<PBAudioStream>(() => new PBAudioStream());
  private pb::UnknownFieldSet _unknownFields;
  private int _hasBits0;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public static pb::MessageParser<PBAudioStream> Parser { get { return _parser; } }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public static pbr::MessageDescriptor Descriptor {
    get { return global::AudioStreamReflection.Descriptor.MessageTypes[0]; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  pbr::MessageDescriptor pb::IMessage.Descriptor {
    get { return Descriptor; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public PBAudioStream() {
    OnConstruction();
  }

  partial void OnConstruction();

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public PBAudioStream(PBAudioStream other) : this() {
    _hasBits0 = other._hasBits0;
    playing_ = other.playing_;
    volume_ = other.volume_;
    url_ = other.url_;
    _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public PBAudioStream Clone() {
    return new PBAudioStream(this);
  }

  /// <summary>Field number for the "playing" field.</summary>
  public const int PlayingFieldNumber = 1;
  private bool playing_;
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

  /// <summary>Field number for the "volume" field.</summary>
  public const int VolumeFieldNumber = 2;
  private float volume_;
  /// <summary>
  /// default=1.0f
  /// </summary>
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public float Volume {
    get { if ((_hasBits0 & 2) != 0) { return volume_; } else { return 0F; } }
    set {
      _hasBits0 |= 2;
      volume_ = value;
    }
  }
  /// <summary>Gets whether the "volume" field is set</summary>
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public bool HasVolume {
    get { return (_hasBits0 & 2) != 0; }
  }
  /// <summary>Clears the value of the "volume" field</summary>
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public void ClearVolume() {
    _hasBits0 &= ~2;
  }

  /// <summary>Field number for the "url" field.</summary>
  public const int UrlFieldNumber = 3;
  private string url_ = "";
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public string Url {
    get { return url_; }
    set {
      url_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
    }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public override bool Equals(object other) {
    return Equals(other as PBAudioStream);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public bool Equals(PBAudioStream other) {
    if (ReferenceEquals(other, null)) {
      return false;
    }
    if (ReferenceEquals(other, this)) {
      return true;
    }
    if (Playing != other.Playing) return false;
    if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Volume, other.Volume)) return false;
    if (Url != other.Url) return false;
    return Equals(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public override int GetHashCode() {
    int hash = 1;
    if (HasPlaying) hash ^= Playing.GetHashCode();
    if (HasVolume) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Volume);
    if (Url.Length != 0) hash ^= Url.GetHashCode();
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
    if (HasPlaying) {
      output.WriteRawTag(8);
      output.WriteBool(Playing);
    }
    if (HasVolume) {
      output.WriteRawTag(21);
      output.WriteFloat(Volume);
    }
    if (Url.Length != 0) {
      output.WriteRawTag(26);
      output.WriteString(Url);
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
    if (HasPlaying) {
      output.WriteRawTag(8);
      output.WriteBool(Playing);
    }
    if (HasVolume) {
      output.WriteRawTag(21);
      output.WriteFloat(Volume);
    }
    if (Url.Length != 0) {
      output.WriteRawTag(26);
      output.WriteString(Url);
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
    if (HasPlaying) {
      size += 1 + 1;
    }
    if (HasVolume) {
      size += 1 + 4;
    }
    if (Url.Length != 0) {
      size += 1 + pb::CodedOutputStream.ComputeStringSize(Url);
    }
    if (_unknownFields != null) {
      size += _unknownFields.CalculateSize();
    }
    return size;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public void MergeFrom(PBAudioStream other) {
    if (other == null) {
      return;
    }
    if (other.HasPlaying) {
      Playing = other.Playing;
    }
    if (other.HasVolume) {
      Volume = other.Volume;
    }
    if (other.Url.Length != 0) {
      Url = other.Url;
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
          Playing = input.ReadBool();
          break;
        }
        case 21: {
          Volume = input.ReadFloat();
          break;
        }
        case 26: {
          Url = input.ReadString();
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
          Playing = input.ReadBool();
          break;
        }
        case 21: {
          Volume = input.ReadFloat();
          break;
        }
        case 26: {
          Url = input.ReadString();
          break;
        }
      }
    }
  }
  #endif

}

#endregion


#endregion Designer generated code
