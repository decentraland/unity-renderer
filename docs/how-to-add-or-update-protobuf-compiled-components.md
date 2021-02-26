### How to add/update protobuf-compiled components 
(instructions with macOS Homebrew package manager)

1. Install protobuf 3.12 (if a different version is already installed you have to uninstall it first, running `brew uninstall protobuf`):
    - edit the desired version config file (use code from https://github.com/Homebrew/homebrew-core/blob/53fb074d235fe0335fa8ee293a3f639f3cdffa45/Formula/protobuf.rb) replacing everything in the following file `code $(brew --repo homebrew/core)/Formula/protobuf.rb`
    - run `HOMEBREW_NO_AUTO_UPDATE=1 brew install protobuf`

2. Edit/add desired component structure at `kernel/packages/shared/proto/engineinterface.proto`

3. In that same directory (`kernel/packages/shared/proto/engineinterface.proto`) run the following command:
`protoc --plugin=../../../node_modules/ts-protoc-gen/bin/protoc-gen-ts --js_out="import_style=commonjs,binary:." --ts_out="." engineinterface.proto`

4. In that same directory run the following command:
`protoc --csharp_out=../../../../unity-client/Assets/Scripts/MainScripts/DCL/Models/Protocol/ --csharp_opt=base_namespace=DCL.Interface engineinterface.proto`

5. Make sure to also update the custom "save/read" code we have at:
    - `kernel/packages/scene-system/scene.system.ts` in `generatePBObject()`
    - `unity-client/Assets/Scripts/MainScripts/DCL/Controllers/Scene/MessageDecoder.cs`
