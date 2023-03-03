# Profiling a WebGL Build

1. Create a folder named `Builds` in the root
1. Open the build settings and toggle on `Development Build`, `Autoconnect Profiler` and **make sure Deep Profiling is toggled off**
3. Build inside the `Builds` folder, and the build name must be named `unity`. The final folder should be `(root)/Builds/unity/`
4. Go to `cd browser-interface` with the console and then `npm install` and `make watch` to execute the browser interface locally. You need to check on the logs if it is copying the Unity build files.
5. Open http://localhost:8080/ and after starting the explorer your Profiler window at Unity Editor should start receiving and displaying the data (you can open on the Unity Editor at `window -> analysis -> profiler`)

# Profiling a Desktop Build

1. Build the project for your target platform with the following build settings checked: `Development Build`, `Autoconnect Profiler` and `Deep Profiling` (this last, if you want deep profiling)
2. Open the build adding the `--without-ssl` parameter
3. After the explorer opens; open on a Chrome Tab to https://play.decentraland.org/?ws=ws://localhost:7666/dcl
4. After starting the explorer your Profiler window at Unity Editor should start receiving and displaying the data (you can open on the Unity Editor at `window -> analysis -> profiler`)

## Important notes and recommendations

- The same Unity Editor instance that made the build **shouldn't be closed**, as that will make the new build useless for profiling, at least at the time of writting this, building in macOS, **it only sends the data to the same Unity Editor instance that built it**.

- Since the profiler data comes from the build, you can't "pause the game" and analyze the data, that should be done toggling the "record" button, keep in mind that it doesn't work as expected when re-toggled, and you probably have to toggle/untoggle it several times to start receiving the data again, in the same session in which the button was untoggled to analyze the data.

- If the data is not transmitted automatically, fiddle with toggling/untoggling the record button in the Profiler until the samples start arriving at Unity

- Recommended setup: Only CPU, Rendering and Memory modules on display; Disable VSync and "Others" from CPU module.
