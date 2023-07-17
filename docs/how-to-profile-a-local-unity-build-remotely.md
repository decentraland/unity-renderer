# Profiling a WebGL Build with Unity Editor
**Important: do not close the Unity Editor that makes the build or the WebGL build won't be able to auto-connect to its profiler, as the building process bakes the current local ip of the Unity Editor instance to auto-connect (it's dynamic so if you close and re-open the editor, its local ip changes and the build won't be able to connect to the new editor instance profiler)**

1. Create a folder named `Builds` in the root
2. In Unity Editor open the build settings. **Toggle on** `Development Build` and `Autoconnect Profiler`. **Important: make sure that `Deep Profiling` is toggled OFF, WebGL Builds do NOT support Deep Profiling**
3. Build inside the `Builds` folder, and the build name must be named `unity`. The final folder should be `(root)/Builds/unity/`
4. After the building process finishes, make sure that inside `(root)/Builds/unity/Build/` the files `unity.data`, `unity.framework.js` and `unity.wasm` exist.
5. Go to `cd browser-interface` with the console and then run `make build-unity-local` and then `make watch` and wait until the make watch server is running and displays the 5 "OK" status messages.
6. Copy the new build files `unity.data`, `unity.framework.js` and `unity.wasm`, that should be at `(root)/Builds/unity/`, and paste them at `(root)/browser-interface/static/`
7. In Unity Editor open the profiler window (`window -> analysis -> profiler`)
8. Open a web browser at http://localhost:8080/ and after starting the explorer your Profiler window at Unity Editor should start receiving and displaying the data. If it doesn't work right away try toggling and untoggling the record button until it starts. (Optional: using http://localhost:8080/?ENABLE_WEB3 you can sign in with a wallet for Web3 interactions)

After that, you can keep re-building and copy-pasting the files, as long as the `make watch` process keeps running the build should be updated without problems.

# Profiling a WebGL Build with Firefox Profiling Tool
It may be useful to use a web browser profiling tool to also see the processess outside the unity build, or the final javascript code run by the unity build.
Firefox profiling tool is better than Chrome's as it allows to see Native code and JS callstacks (unity related stuff) separately.

1. Open Firefox Profiler (F12→Performance)
2. Record samples with one tab being open in the browser only
3. In Call Tree View → select only JavaScript (to see mainly only Unity-related things)
4. In Call Tree View → Invert Call Stack is useful to see which function was the most expensive
5. Calls with more Samples are more representative

**NOTES**
- If function you need to profile has low Samples number, then reduce Samples Rate from 1ms to 0.1/0.001 ms (and with high buffer) and then record just specific parts
- On topbar view - orange is for GC. And red for hiccups
- If it crashes - increase buffer and also Samples Rate (to 2 or 3 ms)

# Profiling a Desktop Build with Unity Editor

1. Build the project for your target platform with the following build settings checked: `Development Build`, `Autoconnect Profiler`. In contrast with WebGL build restrictions, Desktop builds have no restrictions on Deep Profiling.
2. Open the build adding the `--no-ssl` parameter
3. After the explorer opens; open on a Chrome Tab to https://play.decentraland.org/?ws=ws://localhost:7666/dcl
4. After starting the explorer your Profiler window at Unity Editor should start receiving and displaying the data (you can open on the Unity Editor at `window -> analysis -> profiler`)

## Important notes and recommendations for Unity Editor profiling with builds

- Since the profiler data comes from the build, you can't "pause the game" and analyze the data, that should be done toggling the "record" button, keep in mind that it doesn't work as expected when re-toggled, and you probably have to toggle/untoggle it several times to start receiving the data again, in the same session in which the button was untoggled to analyze the data.

- If the data is not transmitted automatically, fiddle with toggling/untoggling the record button in the Profiler until the samples start arriving at Unity

- Recommended setup: Only CPU, Rendering and Memory modules on display; Disable VSync and "Others" from CPU module.
