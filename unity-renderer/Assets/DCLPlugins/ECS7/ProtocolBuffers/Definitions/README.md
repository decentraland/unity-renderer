# How to add a new ECS Component with Protocol-Buffers

In this directory, we have all the components with the schema `ComponentName.proto`. `ComponentName` has to be PascalCase, and the `.proto` is the extension that is recognized as protocol-buffer schema.

Inside each proto, we need to have at least this template:
```proto
syntax = "proto3";

import "common/id.proto";
option (ecs_component_id) = XXXX;

message PBComponentName {
    float one_parameter = 1;
}

```

Some points that must be well-defined:
- `XXXX` the component ID is a super important property of the component, and it has to be unique at least between proto, otherwise, the code generation will fail.
- `PBComponentName` the root message has to be the same as the file name with the prefix `PB`
- `one_parameter` each parameter name has to be snake_case.


## Common directory
The common directory only has to have unambiguous structures like Vector3, Color3, Quaternion, etc. These messages will never change.

## About specific options with other languages
The definition must be the minimal proto code, and it shouldn't have a specific option to compile on some platform. So if you have to add additional information to the proto, you will be able to run a post-process after the protocol schemas acquisition.
For example, in the unity-renderer repo, it'll be necessary to add the `csharp_namespace` option but it's the responsibility of this particular implementation. This code option doesn't define how the component is.

# Test component
If you write a proto and then push, CI will fail because the tests will fail. All the components need to be tested. `@dcl/ecs/test/components` has some examples!
