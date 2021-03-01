## How to debug with local test parcels and preview scenes

### Running Test Scenes

You can test and debug components and SDK functionalities with many test-scenes located at `kernel/public/test-scenes/`, running the client in `debug` mode.

1. Start the regular kernel server in a terminal at '/kernel' and leave it watching for changes

    `make watch`

2. In **another terminal** in '/kernel' run the following command to build the test scenes and keep watching changes on them (changes will be visible when refreshing the browser):

    `make watch-only-test-scenes`

Note that the test-scenes building may take a while (10 mins or more).

3. To run the client in `debug` mode, append the following query parameter to the URL:

    http://localhost:3000/?DEBUG_MODE

4. To spawn at the specific coordinates of a test scene append the position query paramter:

    http://localhost:3000/?DEBUG_MODE&position=-100,100

NOTE(optional): If you don't need to modify the test-scenes and just build them once, you may only run once `make test-scenes` (it takes a while) and that'd be it.

### Creating New Test Scenes
It is possible to define new scenes inside this repo for testing purposes. To do so, create a new folder in `kernel/public/test-scenes`. There are several conventions to be followed regarding the name of these folders and the positions of the parcels, these can be found in the [README](https://github.com/decentraland/client/blob/master/public/test-scenes/README.md) file.

To be able to see the changes made on a test scene (by reloading the browser), without needing to rebuild all the scenes, you should have another terminal running `make watch-only-test-scenes` and that'll keep watching for changes in the test scenes.

### Preview Mode Scenes

#### Unity Editor debugging with dcl scene in "preview mode"

1. Run 'dcl start' to open the scene in preview mode. Leave the server running and close the newly-opened browser tab
2. In Unity Editor, in the "InitialScene" scene, select the WSSController gameobject and untoggle the "Open Browser When Start", and toggle the "Use client debug mode". (Make sure the SceneController has the "Debug Scenes" toggle OFF)
3. In Unity Editor hit PLAY
4. In a new browser tab go to http://localhost:8000/?UNITY_ENABLED=true&DEBUG_MODE&LOCAL_COMMS&position=0%2C-1&ws=ws%3A%2F%2Flocalhost%3A5000%2Fdcl
5. Go back to unity and the scene should start loading in there almost immediately

#### DCL scene in preview mode using a local Kernel version

1. In the explorer repo directory, run a `make watch` once
2. Kill the server and run `make npm-link`
3. In the scene directory run `npm link decentraland-ecs`
4. In the scene directory run `dcl start` and it should already be using the local version of the client
