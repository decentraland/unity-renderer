# Decentraland Unity Renderer

This repository contains the reference implementation of the [decentraland explorer](https://play.decentraland.org). It includes two main big components, located in the folders:

* `unity-renderer` which contains the main 3D experience and UI
* `browser-interface` to connect to the different aspects requiring of a web browser, such as connection with a wallet and WebRTC communications

# Running the Explorer

## Main Dependencies

* Install images and binary files using `git lfs` ([git-lfs.github.com](https://git-lfs.github.com/)). These can be installed from bash or PowerShell by typing:

    git lfs install
    git lfs pull

* The [Unity](https://unity.com) engine and IDE, currently using version 2021.3.14f1
* [node.js](https://nodejs.com), version 16 or later

### Steps

Check: [Multiplatform in Editor](docs/multiplatform-in-editor.md)

1. Download and install Unity 2021.3.14f1
2. Open the scene named `InitialScene`
3. Within the scene, select the `DebugConfig` GameObject.
4. On `DebugConfig` inspector, make sure that `Base url mode` is set to `Custom`
   and `Base url custom` is set to `https://play.decentraland.zone/?`
5. Run the Initial Scene in the Unity editor
6. A browser tab with `explorer` should open automatically and steal your focus, don't close it! Login with your wallet, go back to Unity and explorer should start running on the `Game View`.
7. As you can see, `DebugConfig` has other special options like the starting position, etc. You are welcome to use them as you see fit, but you'll have to close the tab and restart the scene for them to make effect.

### Troubleshooting

#### Missing git lfs extension

If while trying to compile the Unity project you get an error regarding some libraries that can not be added (for instance Newtonsoft
Json.NET or Google Protobuf), please execute the following command in the root folder:

    git lfs install
    git lfs pull

Then, on the Unity editor, click on `Assets > Reimport All`

---

## Testing your branch using automated builds

To test against a build made on this repository, you can use a link with this format:

    https://play.decentraland.zone/?explorer-branch=<branch-name>

# Links for Contributors

1. [Contribution Guidelines](.github/CONTRIBUTING.md)
2. [Coding Guidelines](docs/style-guidelines.md)
3. [Code Review Standards](docs/code-review-standards.md)
4. [Architecture](https://github.com/decentraland/architecture)

# Advanced debugging scenarios

## Running the browser-interface

In order to run browser interface in any platform follow the next instructions

### How to run `make watch`

1. Open browser-interface with `Visual Studio Code`
2. Make sure you have the `devcontainers` extension installed
3. Make sure [Docker Desktop](https://www.docker.com/) is running
4. `At Visual Studio Code` press `F1` execute `Reopen in Container` and wait for it to finish.
5. Go to `Terminal > New Terminal` menu and run `make watch` command. 

## How to run browser-interface unit tests

1. Follow the previous process to run `make watch` 
2. Open `localhost:8080/test` in your browser
3. Watch the results

## Debug with Unity Editor + local Browser Interface

Use this approach when working on any features that need both Browser Interface and Unity modifications, and you need to watch Unity code changes fast without the need of injecting a wasm targeted build in the browser.

When the steps are followed, you will be able to test your changes by just pressing the "Play" button within Unity. This will open a tab running the local Browser Interface build and Unity will connect to it using websocket.

This is the most useful debugging scenario for advanced feature implementation.

### Steps

1. Make sure you have the proper Unity version up and running
3. Make sure you are running browser-interface through `make watch` command on `browser-interface` path.
4. Back in unity editor, open the `DebugConfig` component inspector of `InitialScene`
5. Make sure that the component is setup correctly
6. Hit 'Play' button

## Debug with browsers + local Unity build

This approach works when your Unity modifications run well in the wasm targeted unity build, but you don't want to wait for the CI to kick in. This is also useful for remote profiling.

When the steps are followed, you will be able to run the local Unity build by going to `localhost:3000` without the need of CI.

### Steps

1. Make sure you have the proper Unity version up and running
3. Make sure you are running browser-interface through `make watch` command.
4. Produce a Unity wasm targeted build using the Build menu.
5. When the build finishes, copy all the files inside the resulting `/build` folder (`unity.loader.js` is not necessary as we use a modified loader) and paste them inside `browser-interface/node_modules/@dcl/unity-renderer`.
6. Run the browser explorer through `localhost:8080&ENABLE_WEB3`. Now, it should use your local Unity build. Don't mind the white screen at the beginning, that's because the website repo is not being used and it's only loading Browser Interface with the build.
7. If you need a Unity re-build, you can just replace the files and reload the browser without restarting the `make watch` process.

Alternatively you can go through these 2 steps after step 3 and load the build locally using `localhost:3000` 
1. Make sure you have the [explorer website repository](https://github.com/decentraland/explorer-website) cloned.
2. Make sure you have the local website up and running by executing `npm run start:linked` in the cloned repo directory (`npm i` first just in case).
3. When the WebGL build finishes, copy all the files inside the resulting `/build` folder (`unity.loader.js` is not necessary as we use a modified loader) and paste them inside `explorer-website/node_modules/@dcl/unity-renderer`.
4. Access using `localhost:3000`

### Troubleshooting

#### MacOS: Missing xcrun

If you get the "missing xcrun" error when trying to run the `make watch` command, you should download the latest command line tools for macOS, either by downloading them from https://developer.apple.com/download/more/?=command%20line%20tools or by re-installing XCode

#### MacOS: Build fails throwing `System.ComponentModel.Win32Exception (2): No such file or directory...`

If the local WebGL build always fails with the error `System.ComponentModel.Win32Exception (2): No such file or directory...` it's because you are missing Python needed version (MacOS 12.3 onwards removes the previously-integrated python installation). So to solve this issue just install this [Python version](https://www.python.org/downloads/release/python-3105/).

## Frameworks and Tools

- [UI Components Library](docs/ui-components-library.md)
- [Performance Meter Tool](docs/performance-meter-tool.md)
- [Bots Tool](docs/bots-tool.md)

## Technical how-to guides and explainers

- [How to use Unity visual tests](docs/how-to-use-unity-visual-tests.md)
- [How to profile a local Unity build remotely](docs/how-to-profile-a-local-unity-build-remotely.md)
- [How to connect the Editor with play.decentraland.org](docs/how-to-connect-with-play-decentraland-org.md)
  - For more advanced topics, don't forget to check out our [Architecture Decisions Records](https://github.com/decentraland/adr) (ADR) repository.
- [How to create new components for ECS7](docs/ecs7-component-creation.md)
- [How to modify the Renderer Protocol](docs/how-to-renderer-protocol.md)
- [How to use Asset Bundle Converter](docs/how-to-use-asset-bundle-coverter.md)

## Setup CircleCI

[Setup CircleCI](docs/setup-circleci.md)

## Copyright info

This repository is protected with a standard Apache 2 license. See the terms and conditions in
the [LICENSE](https://github.com/decentraland/unity-renderer/blob/master/LICENSE) file.
