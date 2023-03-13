# Profiling a WebGL Build

1. Create a folder named `Builds` in the root
1. Open the build settings. **Toggle on** `Development Build` and `Autoconnect Profiler`.
2. Also in build settings, make sure that `Deep Profiling` is **toggled off**. **WebGL Builds do NOT support Deep Profiling**
3. Build inside the `Builds` folder, and the build name must be named `unity`. The final folder should be `(root)/Builds/unity/`
4. Ensure the addressables are correctly built, your `StreamingAssets` folder should contain a folder called `aa` which contains folders for WebGL assets. Within the unity editor open `Addressables Groups -> Build -> New Build -> Default Build Script` if they are not already built. They should be copied as part of the docker-compose up. 
5. Go to `cd browser-interface` with the console and then execute `docker-compose up` (requires docker and docker compose) to execute the browser interface locally. You need to check on the logs if it is copying the Unity build files.
6. Open http://localhost:8080/ and after starting the explorer your Profiler window at Unity Editor should start receiving and displaying the data (you can open on the Unity Editor at `window -> analysis -> profiler`).

# Profiling a Desktop Build

1. Build the project for your target platform with the following build settings checked: `Development Build`, `Autoconnect Profiler`. In contrast with WebGL build restrictions, Desktop builds have no restrictions on Deep Profiling.
2. Open the build adding the `--no-ssl` parameter
3. After the explorer opens; open on a Chrome Tab to https://play.decentraland.org/?ws=ws://localhost:7666/dcl
4. After starting the explorer your Profiler window at Unity Editor should start receiving and displaying the data (you can open on the Unity Editor at `window -> analysis -> profiler`)

## Important notes and recommendations

- The same Unity Editor instance that made the build **shouldn't be closed**, as that will make the new build useless for profiling, at least at the time of writting this, building in macOS, **it only sends the data to the same Unity Editor instance that built it**.

- Since the profiler data comes from the build, you can't "pause the game" and analyze the data, that should be done toggling the "record" button, keep in mind that it doesn't work as expected when re-toggled, and you probably have to toggle/untoggle it several times to start receiving the data again, in the same session in which the button was untoggled to analyze the data.

- If the data is not transmitted automatically, fiddle with toggling/untoggling the record button in the Profiler until the samples start arriving at Unity

- Recommended setup: Only CPU, Rendering and Memory modules on display; Disable VSync and "Others" from CPU module.

- The Unity WebGL build process bakes in the `local ip` to the build itself for the process of profiling. "Autoconnect to profiler" will only work if the machine that compiled it has the same IP. If your IP changes, profiling will no longer work and you will have to build again. This only happens when you have "AutoConnect To Profiler" ticked in unity.
