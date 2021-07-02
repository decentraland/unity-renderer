### Profiling a build with Unity Editor

1. Follow the [local build steps](https://github.com/decentraland/unity-renderer#debug-with-browsers--local-unity-build) but for step 4 apply the following settings before building:

  - Open the Player Settings and go to the **compression** setting and set it to **Disabled**

  - Open the build settings and toggle on "Development Build", "Autoconnect Profiler" and **make sure Deep Profiling is toggled off**

2. In Unity Editor open the profiler at window -> analysis -> profiler
3. Since Unity2020, the WebGL development build doesn't produce the same 3 `.unityweb` files, instead there is a `.wasm` file and two `.js` files. Move those files into the corresponding kernel folder (right next to the production `.unityweb` files) and then update (in [unity-interface loader.ts](https://github.com/decentraland/explorer/blob/master/kernel/packages/unity-interface/loader.ts) or in [browser-interface/src/index.ts](https://github.com/decentraland/unity-renderer/blob/master/browser-interface/src/index.ts)) the `config` const properties to point to your newly created build files, like so:
```
dataUrl: resolveWithBaseUrl('unity.data'),
frameworkUrl: resolveWithBaseUrl('unity.framework.js'),
codeUrl: resolveWithBaseUrl('unity.wasm')
```

4. Open the new build (http://localhost:3000/?ENV=org) and after starting the explorer your Profiler window at Unity Editor should start receiving and displaying the data

#### Important notes and recommendations

- The same Unity Editor instance that made the build **shouldn't be closed**, as that will make the new build useless for profiling, at least at the time of writting this, building in macOS, **it only sends the data to the same Unity Editor instance that built it**.

- Since the profiler data comes from the build, you can't "pause the game" and analyze the data, that should be done toggling the "record" button, keep in mind that it doesn't work as expected when re-toggled, and you probably have to toggle/untoggle it several times to start receiving the data again, in the same session in which the button was untoggled to analyze the data.

- If the data is not transmitted automatically, fiddle with toggling/untoggling the record button in the Profiler until the samples start arriving at Unity

- Recommended setup: Only CPU, Rendering and Memory modules on display; Disable VSync and "Others" from CPU module.
