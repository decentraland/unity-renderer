# Decentraland Kernel

## Contributing

**Please read the [contribution guidelines](.github/CONTRIBUTING.md)**

### Important

This repo requires `git lfs` to track images and other binary files. https://git-lfs.github.com/ and the latest version of GNU make, install it using `brew install make`
If you are using Windows 10 we recommend you to enable the Linux subsystem and install a Linux distro from Windows Store like Ubuntu. Then install all tools and dependecies like nodejs, npm, typescript, make...

## Running locally

First, build the project:

    make build

To start hacking, run:

    make watch

To run the client in `debug` mode append the following query parameter to the URL:

    http://localhost:8080/?DEBUG_MODE

To run the client in first person perspective append the following query parameter to the URL:

    http://localhost:8080/?DEBUG_MODE&fps

To spawn in a specific set of coordinates append the following query paramter:

    http://localhost:8080/?DEBUG_MODE&fps&position=10,10

## Running tests

To run all the tests (and save new screenshots), run:

    make generate-images

To see test logs/errors directly in the browser, run:

    make watch and navigate to http://localhost:8080/test

### Visual tests

Visual tests are meant to work in a similar way as `snapshot tests`. Each time a test parcel changes the author is required to commit new screenshots along the other changes. These screenshots are then validated to detect regressions at the time of the pull request. To generate new snapshot images to compare run `npm run test:dry` (it requires docker)

### Test parcels

It is possible to define new parcels inside this repo for testing purposes. To do so, create a new folder in `public/test-scenes`. There are several conventions to be followed regarding the name of these folders and the positions of the parcels, these can be found in the [README](https://github.com/decentraland/client/blob/master/public/test-scenes/README.md) file.

To edit and make sure that `make watch` is rebuilding the scene when you are hacking on a new feature of the kernel, make sure to modify `targets/scenes/basic-scenes.json` and point to the scene you're working on.

All test parcels can be accessed inside visual tests:

```ts
import { loadTestParcel } from 'test/testHelpers'

describe('My example test', function() {
  loadTestParcel(200, 10)
  // ...
```

### Testing with different renderers (unity engine builds)

1. Visit https://www.npmjs.com/package/decentraland-renderer -> “versions” tab and choose the wanted deployment tag
2. In the repo's root directory run the following command: `npm install decentraland-renderer@[CHOSEN TAG]`
3. Run `make watch` and the local kernel should already use the selected renderer (make sure to restart it if you are already running it).
4. Whenever the `npm` tag is changed and you would like to update that, run `npm install decentraland-renderer@[CHOSEN TAG]` again!

## Copyright info

This repository is protected with a standard Apache 2 license. See the terms and conditions in the [LICENSE](https://github.com/decentraland/client/blob/master/LICENSE) file.
