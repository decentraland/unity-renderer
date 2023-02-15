// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: decentraland/renderer/kernel_services/sign_request.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Decentraland.Renderer.KernelServices {

  /// <summary>Holder for reflection information generated from decentraland/renderer/kernel_services/sign_request.proto</summary>
  public static partial class SignRequestReflection {

    #region Descriptor
    /// <summary>File descriptor for decentraland/renderer/kernel_services/sign_request.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static SignRequestReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CjhkZWNlbnRyYWxhbmQvcmVuZGVyZXIva2VybmVsX3NlcnZpY2VzL3NpZ25f",
            "cmVxdWVzdC5wcm90bxIlZGVjZW50cmFsYW5kLnJlbmRlcmVyLmtlcm5lbF9z",
            "ZXJ2aWNlcyJXChBTaWduQm9keVJlc3BvbnNlEhUKDWF1dGhfaGVhZGVyXzEY",
            "ASABKAkSFQoNYXV0aF9oZWFkZXJfMhgCIAEoCRIVCg1hdXRoX2hlYWRlcl8z",
            "GAMgASgJImYKD1NpZ25Cb2R5UmVxdWVzdBIOCgZtZXRob2QYASABKAkSEAoI",
            "YmFzZV91cmwYAiABKAkSDAoEcGF0aBgDIAEoCRIRCgl0aW1lc3RhbXAYBCAB",
            "KAUSEAoIbWV0YWRhdGEYBSABKAkypQEKGFNpZ25SZXF1ZXN0S2VybmVsU2Vy",
            "dmljZRKIAQoTR2V0UmVxdWVzdFNpZ25hdHVyZRI2LmRlY2VudHJhbGFuZC5y",
            "ZW5kZXJlci5rZXJuZWxfc2VydmljZXMuU2lnbkJvZHlSZXF1ZXN0GjcuZGVj",
            "ZW50cmFsYW5kLnJlbmRlcmVyLmtlcm5lbF9zZXJ2aWNlcy5TaWduQm9keVJl",
            "c3BvbnNlIgBiBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Decentraland.Renderer.KernelServices.SignBodyResponse), global::Decentraland.Renderer.KernelServices.SignBodyResponse.Parser, new[]{ "AuthHeader1", "AuthHeader2", "AuthHeader3" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Decentraland.Renderer.KernelServices.SignBodyRequest), global::Decentraland.Renderer.KernelServices.SignBodyRequest.Parser, new[]{ "Method", "BaseUrl", "Path", "Timestamp", "Metadata" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class SignBodyResponse : pb::IMessage<SignBodyResponse>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<SignBodyResponse> _parser = new pb::MessageParser<SignBodyResponse>(() => new SignBodyResponse());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<SignBodyResponse> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Decentraland.Renderer.KernelServices.SignRequestReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SignBodyResponse() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SignBodyResponse(SignBodyResponse other) : this() {
      authHeader1_ = other.authHeader1_;
      authHeader2_ = other.authHeader2_;
      authHeader3_ = other.authHeader3_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SignBodyResponse Clone() {
      return new SignBodyResponse(this);
    }

    /// <summary>Field number for the "auth_header_1" field.</summary>
    public const int AuthHeader1FieldNumber = 1;
    private string authHeader1_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string AuthHeader1 {
      get { return authHeader1_; }
      set {
        authHeader1_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "auth_header_2" field.</summary>
    public const int AuthHeader2FieldNumber = 2;
    private string authHeader2_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string AuthHeader2 {
      get { return authHeader2_; }
      set {
        authHeader2_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "auth_header_3" field.</summary>
    public const int AuthHeader3FieldNumber = 3;
    private string authHeader3_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string AuthHeader3 {
      get { return authHeader3_; }
      set {
        authHeader3_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as SignBodyResponse);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(SignBodyResponse other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (AuthHeader1 != other.AuthHeader1) return false;
      if (AuthHeader2 != other.AuthHeader2) return false;
      if (AuthHeader3 != other.AuthHeader3) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (AuthHeader1.Length != 0) hash ^= AuthHeader1.GetHashCode();
      if (AuthHeader2.Length != 0) hash ^= AuthHeader2.GetHashCode();
      if (AuthHeader3.Length != 0) hash ^= AuthHeader3.GetHashCode();
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
      if (AuthHeader1.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(AuthHeader1);
      }
      if (AuthHeader2.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(AuthHeader2);
      }
      if (AuthHeader3.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(AuthHeader3);
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
      if (AuthHeader1.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(AuthHeader1);
      }
      if (AuthHeader2.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(AuthHeader2);
      }
      if (AuthHeader3.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(AuthHeader3);
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
      if (AuthHeader1.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(AuthHeader1);
      }
      if (AuthHeader2.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(AuthHeader2);
      }
      if (AuthHeader3.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(AuthHeader3);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(SignBodyResponse other) {
      if (other == null) {
        return;
      }
      if (other.AuthHeader1.Length != 0) {
        AuthHeader1 = other.AuthHeader1;
      }
      if (other.AuthHeader2.Length != 0) {
        AuthHeader2 = other.AuthHeader2;
      }
      if (other.AuthHeader3.Length != 0) {
        AuthHeader3 = other.AuthHeader3;
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
            AuthHeader1 = input.ReadString();
            break;
          }
          case 18: {
            AuthHeader2 = input.ReadString();
            break;
          }
          case 26: {
            AuthHeader3 = input.ReadString();
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
            AuthHeader1 = input.ReadString();
            break;
          }
          case 18: {
            AuthHeader2 = input.ReadString();
            break;
          }
          case 26: {
            AuthHeader3 = input.ReadString();
            break;
          }
        }
      }
    }
    #endif

  }

  public sealed partial class SignBodyRequest : pb::IMessage<SignBodyRequest>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<SignBodyRequest> _parser = new pb::MessageParser<SignBodyRequest>(() => new SignBodyRequest());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<SignBodyRequest> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Decentraland.Renderer.KernelServices.SignRequestReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SignBodyRequest() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SignBodyRequest(SignBodyRequest other) : this() {
      method_ = other.method_;
      baseUrl_ = other.baseUrl_;
      path_ = other.path_;
      timestamp_ = other.timestamp_;
      metadata_ = other.metadata_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SignBodyRequest Clone() {
      return new SignBodyRequest(this);
    }

    /// <summary>Field number for the "method" field.</summary>
    public const int MethodFieldNumber = 1;
    private string method_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string Method {
      get { return method_; }
      set {
        method_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "base_url" field.</summary>
    public const int BaseUrlFieldNumber = 2;
    private string baseUrl_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string BaseUrl {
      get { return baseUrl_; }
      set {
        baseUrl_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "path" field.</summary>
    public const int PathFieldNumber = 3;
    private string path_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string Path {
      get { return path_; }
      set {
        path_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "timestamp" field.</summary>
    public const int TimestampFieldNumber = 4;
    private int timestamp_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int Timestamp {
      get { return timestamp_; }
      set {
        timestamp_ = value;
      }
    }

    /// <summary>Field number for the "metadata" field.</summary>
    public const int MetadataFieldNumber = 5;
    private string metadata_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string Metadata {
      get { return metadata_; }
      set {
        metadata_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as SignBodyRequest);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(SignBodyRequest other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Method != other.Method) return false;
      if (BaseUrl != other.BaseUrl) return false;
      if (Path != other.Path) return false;
      if (Timestamp != other.Timestamp) return false;
      if (Metadata != other.Metadata) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (Method.Length != 0) hash ^= Method.GetHashCode();
      if (BaseUrl.Length != 0) hash ^= BaseUrl.GetHashCode();
      if (Path.Length != 0) hash ^= Path.GetHashCode();
      if (Timestamp != 0) hash ^= Timestamp.GetHashCode();
      if (Metadata.Length != 0) hash ^= Metadata.GetHashCode();
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
      if (Method.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Method);
      }
      if (BaseUrl.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(BaseUrl);
      }
      if (Path.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(Path);
      }
      if (Timestamp != 0) {
        output.WriteRawTag(32);
        output.WriteInt32(Timestamp);
      }
      if (Metadata.Length != 0) {
        output.WriteRawTag(42);
        output.WriteString(Metadata);
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
      if (Method.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Method);
      }
      if (BaseUrl.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(BaseUrl);
      }
      if (Path.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(Path);
      }
      if (Timestamp != 0) {
        output.WriteRawTag(32);
        output.WriteInt32(Timestamp);
      }
      if (Metadata.Length != 0) {
        output.WriteRawTag(42);
        output.WriteString(Metadata);
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
      if (Method.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Method);
      }
      if (BaseUrl.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(BaseUrl);
      }
      if (Path.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Path);
      }
      if (Timestamp != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Timestamp);
      }
      if (Metadata.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Metadata);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(SignBodyRequest other) {
      if (other == null) {
        return;
      }
      if (other.Method.Length != 0) {
        Method = other.Method;
      }
      if (other.BaseUrl.Length != 0) {
        BaseUrl = other.BaseUrl;
      }
      if (other.Path.Length != 0) {
        Path = other.Path;
      }
      if (other.Timestamp != 0) {
        Timestamp = other.Timestamp;
      }
      if (other.Metadata.Length != 0) {
        Metadata = other.Metadata;
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
            Method = input.ReadString();
            break;
          }
          case 18: {
            BaseUrl = input.ReadString();
            break;
          }
          case 26: {
            Path = input.ReadString();
            break;
          }
          case 32: {
            Timestamp = input.ReadInt32();
            break;
          }
          case 42: {
            Metadata = input.ReadString();
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
            Method = input.ReadString();
            break;
          }
          case 18: {
            BaseUrl = input.ReadString();
            break;
          }
          case 26: {
            Path = input.ReadString();
            break;
          }
          case 32: {
            Timestamp = input.ReadInt32();
            break;
          }
          case 42: {
            Metadata = input.ReadString();
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
