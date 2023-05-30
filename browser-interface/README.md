# Decentraland Explorer

This is the [decentraland explorer](https://play.decentraland.org) official repository.

This repository covers mainly the Browser Interface component of the explorer product. 

The Browser Interface component responsibility includes running the SDK scenes, handling back-end business logic and more. This said, you can clone this repo and run the browser version of Explorer. If you want to contribute to our renderer, please check out the unity-renderer repo: https://github.com/decentraland/unity-renderer/pulls. Unity-renderer repo outputs a npm package that's used on this one for publishing the Explorer.

## Before you start

1. [Contribution Guidelines](.github/CONTRIBUTING.md)
2. [Coding Guidelines](docs/style-guidelines.md)
3. [Code Review Standards](docs/code-review-standards.md)

# Running the Explorer

**IMPORTANT:** If your path has spaces the build process will fail. Make sure to clone this repo in a properly named path.

## Manually (Mac/Linux)
Make sure you have the following dependencies:

- Latest version of GNU make, install it using `brew install make`
- Node v10 or compatible installed via `sudo apt install nodejs`, [nvm](https://github.com/nvm-sh/nvm) or [fnm](https://github.com/Schniz/fnm)

### With Docker (Windows/Mac/Linux) (Recommended)

- Install docker (https://www.docker.com/products/docker-desktop/)
- Navigate to project root and run: ``docker compose up``

---

When all the dependencies are in place, you can start building the project.

First off, we need the npm package dependencies. In most of the cases this should be done only once:

    npm install

The next step will require copying different files from the unity build into the target location:

    make build-unity-local

By now, you can run and watch a server with the Browser Interface build by typing:

    make watch

The make process will take a while. When its finished, you can start debugging the browser's explorer by going to http://localhost:8080/ (or http://localhost:8080/?ENABLE_WEB3 if web3 log in is needed) 

### Run browser interface tests

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

## Technical how-to guides and explainers

- [How to create new SDK components](docs/how-to-create-new-sdk-components.md)
- [How to debug with local test parcels and preview scenes](docs/how-to-test-parcels-and-preview-scenes.md)
- [How to use Unity visual tests](docs/how-to-use-unity-visual-tests.md)
- [Kernel-unity native interface explainer and maintenance guide](docs/kernel-unity-native-interface-explainer.md)
- [How to create typescript workers](docs/how-to-create-typescript-workers.md)
- [How to add/update protobuf-compiled components](docs/how-to-add-or-update-protobuf-compiled-components.md)

For more advanced topics, don't forget to check out our [Architecture Decisions Records](https://github.com/decentraland/adr) (ADR) repository.

## Copyright info

This repository is protected with a standard Apache 2 license. See the terms and conditions in the [LICENSE](https://github.com/decentraland/unity-client/blob/master/LICENSE) file.
