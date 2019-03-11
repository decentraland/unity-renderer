[![CircleCI](https://circleci.com/gh/decentraland/explorer.svg?style=svg)](https://circleci.com/gh/decentraland/explorer)

# Refactor victims

- MessageBusController

# Decentraland Client

This client is fully front-end, but uses a [WebRTC Signalling Server](https://github.com/decentraland/rendezvous) to establish connections to other users.

## Contributing

**Please read the [contribution guidelines](.github/CONTRIBUTING.md)**

### Important

This repo requires `git lfs` to track images and other binary files. https://git-lfs.github.com/ and the latest version of GNU make, install it using `brew install make`

## Running locally

First, build the project:

    `make build`

To start hacking, run:

    `make watch`

To run the client in `debug` mode append the following query parameter to the URL:

    http://localhost:8080/?DEBUG

To run the client in first person perspective append the following query parameter to the URL:

    http://localhost:8080/?DEBUG&fps

To spawn in a specific set of coordinates append the following query paramter:

    http://localhost:8080/?DEBUG&fps&position=10,10

## Running tests

To run all test (and save new screenshots), run:

    `make generate-images`

To see test logs/errors directly in the browser, run:

    `make watch` and navigate to http://localhost:8080/test

### Visual tests

Visual tests are meant to work in a similar way as `snapshot tests`. Each time a test parcel changes the author is required to commit new screenshots along the other changes. These screenshots are then validated to detect regressions at the time of the pull request. To generate new snapshot images to compare run `npm run test:dry` (it requires docker)

### Test parcels

It is possible to define new parcels inside this repo for testing purposes. To do so, create a new folder in `public/test-parcels`. There are several conventions to be followed regarding the name of these folders and the positions of the parcels, these can be found in the [README](https://github.com/decentraland/client/blob/master/public/test-parcels/README.md) file.

All test parcels must be registered in the `mock.json` file located in the same folder. Using the `test-local:` prefix means that the parcel will only be available while running the client locally.

All test parcels can be accessed inside visual tests:

```ts
import { loadTestParcel } from 'test/testHelpers'

describe('My example test', function() {
  loadTestParcel(200, 10)
  // ...
```

# To update babylon version

Run `./scripts/updateBabylon.sh VERSION`, usually to develop we run `./scripts/updateBabylon.sh preview`

For releases we should run `./scripts/updateBabylon.sh latest` before.

> Requires `jq` install using `brew install jq`

## Copyright info
This repository is protected with a standard Apache 2 license. See the terms and conditions in the [LICENSE](https://github.com/decentraland/client/blob/master/LICENSE) file.
