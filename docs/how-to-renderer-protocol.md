# Renderer Protocol

## What?

Renderer Protocol is the messages that are sent between the [Kernel](http://github.com/decentraland/kernel) and the [Renderer](http://github.com/decentraland/unity-renderer).

Those messages are defined in the [Decentraland Protocol](https://github.com/decentraland/protocol/tree/main/renderer-protocol) in proto3 format.

## How to add a message

To add a message in the Renderer Protocol, you must change the [Decentraland Protocol](https://github.com/decentraland/protocol/tree/main/renderer-protocol), and then update the package in the `protocol-gen` of the `unity-renderer` repository [here](https://github.com/decentraland/unity-renderer/tree/dev/protocol-gen).

Example of messages
```
syntax = "proto3";
import "google/protobuf/empty.proto";

message Teleport {
  message FromKernel {
    message RequestTeleport {
      string destination = 1;
    }
  }

  message FromRenderer {
    oneof message {
      TeleportTo teleport_to = 3;
      TeleportToCrowd teleport_to_crowd = 4;
      TeleportToMagic teleport_to_magic = 5;
      JumpIn jump_in = 6;
    }

    message TeleportTo {
      int32 x = 1;
      int32 y = 2;
    }

    message TeleportToMagic {}
    message TeleportToCrowd {}

    message JumpIn {
      string realm = 1;
      int32 parcel_x = 2;
      int32 parcel_y = 3;
    }
  }
}
```

After that, just run `npm run build-renderer-protocol` and the Renderer Protocol will be re-generated.

## RPC

The Renderer works as a `RPC Server` that is connected by the Kernel, the `RPC Client`.

> **_NOTE:_**  You can read the following articles to understand RPC [article 1](https://www.techtarget.com/searchapparchitecture/definition/Remote-Procedure-Call-RPC); [article 2](https://grpc.io/docs/what-is-grpc/introduction/)

### Example

In order to execute RPC Procedures from the Kernel to the Renderer, you must add a Service in the same protobuf.

```
service TeleportService {
  rpc RequestTeleport(Teleport.FromKernel.RequestTeleport) returns (google.protobuf.Empty) {}
  rpc OnMessage(google.protobuf.Empty) returns (stream Teleport.FromRenderer) {}
}
```

Here you can execute from the Kernel to the Renderer the `RequestTeleport`.

And to receive messages, you subscribe the `OnMessage functions` that receives one of the `Teleport.FromRenderer` messages.

### Messages from the Renderer to the Kernel

If you want to send multiple messages to the Kernel, you have two options.

#### 1. Create a stream message from the Kernel, to receive all messages you want

#### 2. Create an event list and consume that list from the Kernel