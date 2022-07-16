#Forked repo
# Decentraland Unity Renderer

This repository contains the Unity part of [decentraland explorer](https://play.decentraland.org). This component works alongside Kernel to produce an Explorer build.

## Before you start

1. [Contribution Guidelines](.github/CONTRIBUTING.md)
2. [Coding Guidelines](docs/style-guidelines.md)
3. [Code Review Standards](docs/code-review-standards.md)
4. [Architecture](https://github.com/decentraland/architecture)

# Running the Explorer

## Main Dependencies

This repo requires `git lfs` to track images and other binary files. https://git-lfs.github.com/ .
So, before anything make sure you have it installed by typing:

    git lfs install
    git lfs pull

---

## Debug using Unity

Take this path if you intend to contribute on features without the need of modifying Kernel.
This is the recommended path for artists.

### Steps

1. Download and install Unity 2020.3.0f1
2. Open the scene named `InitialScene`
3. Within the scene, select the `DebugConfig` GameObject.
4. On `DebugConfig` inspector, make sure that `Base url mode` is set to `Custom`
   and `Base url custom` is set to `https://play.decentraland.zone/?`
5. Run the Initial Scene in the Unity editor
6. A browser tab with `explorer` should open automatically and steal your focus, don't close it!. Login with your wallet, go back to Unity and explorer should start running on the `Game View`.
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

    https://play.decentraland.zone/?renderer-branch=<branch-name>

Note that using this approach, the Unity builds will run against kernel `master` HEAD.

If you want to test your Unity branch against a specific kernel branch, you'll have to use the `renderer` url param like this:

    https://play.decentraland.zone/?renderer-branch=<branch-name>&kernel-branch=<kernel-branch-name>

If the CI for both branches succeeds, you can browse to the generated link and test your changes. Bear in mind that any push will kick the CI. There's no need to create a pull request.

---

<a name="advanced-debugging-scenarios"></a>

# Advanced debugging scenarios

## Debug with Unity Editor + local Kernel

Use this approach when working on any features that need both Kernel and Unity modifications, and you need to watch Unity code changes fast without the need of injecting a wasm targeted build in the browser.

When the steps are followed, you will be able to test your changes with just pressing the "Play" button within Unity. This will open a tab running the local Kernel build and Unity will connect to it using websocket.

This is the most useful debugging scenario for advanced feature implementation.

### Steps

1. Make sure you have the proper Unity version up and running
2. Make sure you have Kernel repository cloned and set up.
3. Make sure you are running kernel through `make watch` command.
4. Back in unity editor, open the `DebugConfig` component inspector of `InitialScene`
5. Make sure that the component is setup correctly
6. Hit 'Play' button

## Debug with browsers + local Unity build

This approach works when your Unity modifications run well in the wasm targeted unity build, but you don't want to wait for the CI to kick in. This is also useful for remote profiling.

When the steps are followed, you will be able to run the local Unity build by going to `localhost:3000` without the need of CI.

### Steps

1. Make sure you have the proper Unity version up and running
2. Make sure you have [Kernel repository](https://github.com/decentraland/kernel) cloned.
3. Make sure you are running kernel through `make watch` command in the cloned repo directory (`npm i` first just in case).
4. Produce a Unity wasm targeted build using the Build menu.
5. When the build finishes, copy all the files inside the resulting `/build` folder (`unity.loader.js` is not necessary as we use a modified loader) and paste them inside `kernel/node_modules/@dcl/unity-renderer`.
6. Run the browser explorer through `localhost:8080&ENABLE_WEB3`. Now, it should use your local Unity build. Don't mind the white screen at the beginning, that's because the website repo is not being used and it's only loading Kernel with the build.
7. If you need a Unity re-build, you can just replace the files and reload the browser without restarting the `make watch` process.

Alternatively you can go through these 2 steps after step 3 and load the build locally using `localhost:3000` 
1. Make sure you have the [explorer website repository](https://github.com/decentraland/explorer-website) cloned.
2. Make sure you have the local website up and running by executing `npm run start:linked` in the cloned repo directory (`npm i` first just in case).
3. When the WebGL build finishes, copy all the files inside the resulting `/build` folder (`unity.loader.js` is not necessary as we use a modified loader) and paste them inside `explorer-website/node_modules/@dcl/unity-renderer`.
4. Access using using `localhost:3000`

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

## Setup CircleCI

[Setup CircleCI](docs/setup-circleci.md)

## Copyright info

This repository is protected with a standard Apache 2 license. See the terms and conditions in
the [LICENSE](https://github.com/decentraland/unity-renderer/blob/master/LICENSE) file.
