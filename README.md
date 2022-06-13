# Decentraland Unity Renderer

This repository contains the Unity part of [decentraland explorer](https://play.decentraland.org). This component works alongside Kernel to produce an Explorer build.

## Before you start

1. [Contribution Guidelines](.github/CONTRIBUTING.md)
2. [Coding Guidelines](docs/style-guidelines.md)
3. [Code Review Standards](docs/code-review-standards.md)

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
3. Make sure you are running kernel through `make watch` command in the cloned repo directory.
4. Make sure you have the [explorer website repository](https://github.com/decentraland/explorer-website) cloned.
5. Make sure you have the local website up and running by executing `npm run start:linked` in the cloned repo directory.
6. Produce a Unity wasm targeted build using the Build menu.
7. When the build finishes, copy all the files inside the resulting `/build` folder (`unity.loader.js` is not necessary as we use a modified loader) and paste them inside `explorer-website/node_modules/@dcl/unity-renderer`.
8. Run the browser explorer through `localhost:3000`. Now, it should use your local Unity build.
9. If you need a Unity re-build, you can just replace the files and reload the browser without restarting the `make watch` process nor the website.

## Technical how-to guides and explainers

- [How to use Unity visual tests](docs/how-to-use-unity-visual-tests.md)
- [How to profile a local Unity build remotely](docs/how-to-profile-a-local-unity-build-remotely.md)
- [How to connect the Editor with play.decentraland.org](docs/how-to-connect-with-play-decentraland-org.md)
For more advanced topics, don't forget to check out our [Architecture Decisions Records](https://github.com/decentraland/adr) (ADR) repository.
- [How to create new components] (docs/ecs7-component-creation.md)

## Setup CircleCI

[Setup CircleCI](docs/setup-circleci.md)

## Copyright info

This repository is protected with a standard Apache 2 license. See the terms and conditions in
the [LICENSE](https://github.com/decentraland/unity-renderer/blob/master/LICENSE) file.
