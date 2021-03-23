# Decentraland Explorer

This is the [decentraland explorer](https://play.decentraland.org) official repository.

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

## Debug using Unity only

### Why you should care

Take this path if you intend to contribute on features without the need of modifying Kernel.
This is the recommended path for artists.

### Steps

1. Download and install Unity 2019.4.19f1
2. Open the scene named `InitialScene`
3. Within the scene, select the `WSSController` GameObject.
4. On `WSSController` inspector, make sure that `Base url mode` is set to `Custom` 
and `Base url custom` is set to `https://play.decentraland.zone/?`
5. Run the Initial Scene in the Unity editor
6. A browser tab with `explorer` should open automatically and steal your focus, don't close it!. Login with your wallet, go back to Unity and explorer should start running on the `Game View`.
7. As you can see, `WSSController` has other special options like the starting position, etc. You are welcome to use them as you see fit, but you'll have to close the tab and restart the scene for them to make effect.

### Troubleshooting

#### Missing git lfs extension
If while trying to compile the Unity project you get an error regarding some libraries that can not be added (for instance Newtonsoft
Json.NET or Google Protobuf), please execute the following command in the root folder:

    git lfs install
    git lfs pull

Then, on the Unity editor, click on `Assets > Reimport All`

---

## Debug using Kernel only

### Why you should care

Kernel mostly takes care of scene code execution and sandboxing, communication with catalysts, external services, comms, etc. If you want to delve into any of this, you may want to follow these steps and don't bother with the Unity installation.

### Steps

Make sure you have the following dependencies:
- Latest version of GNU make, install it using `brew install make`
- If you are using Windows 10, you must enable the Linux subsystem and install a Linux distro from Windows Store like Ubuntu. Then install all tools and dependecies like nodejs, npm, typescript, make, et cetera.
- Node v10 or compatible installed via `sudo apt install nodejs` or [nvm](https://github.com/nvm-sh/nvm)

---
**IMPORTANT:** If your path has spaces the build process will fail. Make sure to clone this repo in a properly named path.

---
When all the dependencies are in place, you can start building the project.

First off, we need the npm packages for **website** and **kernel**. In most of the cases this should be done only once:

    cd website
    npm install
    cd kernel
    npm install

By now, you can run and watch a server with the kernel build by typing:

    make watch

The make process will take a while. When its finished, you can start debugging the browser's explorer by going to http://localhost:3000/

Note that the Unity version used by this approach will be the latest version deployed to `master` branch. If you need a local Unity build, check out the [advanced debugging scenarios](#advanced-debugging-scenarios).

### Run kernel tests

To see test logs/errors directly in the browser, run:

    make watch

Now, navigate to [http://localhost:8080/test](http://localhost:8080/test)

### Troubleshooting

#### Missing xcrun (macOS)
If you get the "missing xcrun" error when trying to run the `make watch` command, you should download the latest command line tools for macOS, either by downloading them from https://developer.apple.com/download/more/?=command%20line%20tools or by re-installing XCode

---

## Testing your branch using automated builds

When any commit is pushed to a branch on the server, a build is generated and deployed to:
    
    https://play.decentraland.zone/branch/<branch-name>/index.html

If the CI succeeds, you can browse to the generated link and test your changes. Bear in mind that any push will kick the CI, and there's no need to create a pull request.

---
<a name="advanced-debugging-scenarios"></a>
# Advanced debugging scenarios

## Debug with Unity Editor + local Kernel

### Why you should care

You may want to use this approach for any features that need both Kernel and Unity modifications, and you need to watch Unity code changes fast without the need of injecting a wasm targeted build in the browser. 

When the steps are followed, you will be able to test your changes with just pressing the "Play" button within Unity. This will open a tab running the local Kernel build and Unity will connect to it using websocket. 

This is the most useful debugging scenario for advanced feature implementation.

### Steps

* Make sure you have the proper Unity version up and running
* Make sure you are running kernel through `make watch` command.
* Back in unity editor, open the `WSSController` component inspector of `InitialScene`
* Make sure that is setup correctly

## Debug with browsers + local Unity build

### Why you should care

Use this approach if you want to ensure your Unity modifications run well in the wasm targeted unity build, but don't want to wait for the CI to kick in. This is also useful for remote profiling. 

When the steps are followed, you will be able to run the local Unity build by going to `localhost:3000` without the need of CI.

### Steps

1. Make sure you have the proper Unity version up and running
2. Make sure you are running kernel through `make watch` command.
3. Produce a Unity wasm targeted build using the Build menu.
4. When the build finishes, only copy all the files with the `unityweb` extension to `static/unity/Build` folder within the `kernel` project. **Do not** copy the `UnityLoader.js` and `unity.json` files.
5. Run the browser explorer through `localhost:3000`. Now, it should use your local Unity build.
6. If you need a Unity re-build, you can just replace the files and reload the browser the without restarting the `make watch` process.

## Technical how-to guides and explainers

- [How to create new SDK components](docs/how-to-create-new-sdk-components.md)
- [How to debug with local test parcels and preview scenes](docs/how-to-test-parcels-and-preview-scenes.md)
- [How to use Unity visual tests](docs/how-to-use-unity-visual-tests.md)
- [How to profile a local Unity build remotely](docs/how-to-profile-a-local-unity-build-remotely.md)
- [Kernel-unity native interface explainer and maintenance guide](docs/kernel-unity-native-interface-explainer.md)
- [How to create typescript workers](docs/how-to-create-typescript-workers.md)
- [How to add/update protobuf-compiled components](docs/how-to-add-or-update-protobuf-compiled-components.md)

For more advanced topics, don't forget to check out our [Architecture Decisions Records](https://github.com/decentraland/adr) (ADR) repository. 

## Copyright info

This repository is protected with a standard Apache 2 license. See the terms and conditions in the [LICENSE](https://github.com/decentraland/unity-client/blob/master/LICENSE) file.
