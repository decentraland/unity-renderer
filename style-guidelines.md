# Coding Guidelines

Code is written once, and read many times. Optimize for readability! When in doubt, follow this rule:

> Read out loud the line you just wrote. How far is it from normal, semantically correct English? Would someone not familiar with the code understand what it does?

## Coding Style

For all our style needs we are using an [`.editorconfig`](https://editorconfig.org/) file.

It's recommended to use a `Format On Save` extension on your IDE of choice so we avoid styling feedback noise on pull requests.

We don't use the `.resharper` extensions for `.editorconfig`. So if you use `resharper` plugin or `rider` you must set the `resharper` specific style settings to match those of `VS Code` and `VS Community`.

## Rider

You can find a settings export file in the root of the project called "rider_codeStyleDCLSetting". Bear in mind that the conversion between VS and Rider is not 1 on 1 but it's good enough.

## browser-interface

### Code order

```
imports

local constants

main function, class, or exports

other exports

non-exported functions
```
### redux

* `actions.ts` should contain, for each action the system or the user triggers:
  - A constant definition of the action affecting the system. The name of the module is optional, between square brackets:
    ```
    export const CREATE_NEW_USER_REQUEST = '[module] Request: Create new user'
    ```
  - An action creator, using `typesafe-actions`:
    ```
    export const createNewUser = (userParams: { name: string }) => action(CREATE_NEW_USER_REQUEST, userParams)
    ```
  - The exported type of the action creator:
    ```
    export type CreateNewUserRequest = ReturnType<typeof createNewUserRequest>
    ```
  - It is OK to avoid one of these three if it's not used, but this is likely a warning sign. When one of these is not used, it's likely that the use of `redux-saga` is not in line with the style followed in `decentraland-dapps`.
* `actions.ts` should not contain other exports that don't fall in those three categories (action type string name, action creator, or type of the action creator)
* On naming conventions for actions:
  - Please follow `Request`, `Success` and `Failure` for common HTTP or other RPC-style processes.

### redux-sagas

* If there are three or more consecutive selects that can be merged into one, do it.
* Remember to type the result of `yield select(fun)` by doing `as ReturnType<typeof fun>` (and enclose `(yield select(...))` with parenthesis due to `yield` not having enough precedence).
Don't:
```
function* mySaga() {
  const a = yield select(aSelector)
  const b = yield select(bSelector)
  const c = yield select(cSelector)
  ...
}
```
Do:
```
function* mySaga() {
  const { a, b, c } = (yield select(allInfo)) as ReturnType<typeof allInfo>
}
function allInfo(state) {
  return {
    a: aSelector(state),
    b: bSelector(state),
    c: cSelector(state),
  }
}
```

### module imports

- Always use an absolute path when requiring a module in another subfolder of the `packages` folder.
  * For example, from `shared/comms/index.ts`, the correct way to import `packages/lib/logger/perf.ts` is `import { tick } from 'lib/logger/perf'`.
- Avoid importing from "index".
  * Wrong: `import { sendPublicChatMessage } from 'shared/comms/index'`
  * Right: `import { sendPublicChatMessage } from 'shared/comms'`
- Always use an absolute path to require a module in another subfolder of `packages/shared`
  * For example, to import `sendPublicChatMessage`, placed in the file `shared/comms/index.ts`, from the file `shared/store/index.ts`, use `import { sendPublicChatMessage } from 'shared/comms'`
- Always prefer to use `import type` when possible
- Avoid adding code to `index.ts` files, except for strictly external interfaces
  * This prevents `import { something } from '.'` and circular dependencies.

### code reuse and libraries

- Prefer local code in `lib/` rather than included libraries (even if imported from `@dcl/*`)
  * For example, prefer `lib/math/Vector3` rather than `@dcl/ecs-math/ReadOnlyVector3`
  * Even for small functionality, such as `${x},${y}` to encode a parcel position, prefer using `lib/decentraland/parcels/positionToString`
- Prefer using `jsonFetch` instead of `fetch(...).json()`
  * This allows better readability unless you will explicitely handle error cases without `try { ... } catch { ... }` (for example, to handle HTTP return codes)

### async/await

- Always prefer `await Promise.all([a, b])` rather than `await a; await b;`.
  * The less turnaround, the better and faster the experience. This really compounds!

### testability

- "Inversion of control": Always prefer arguments rather than accesses to global objects.
  * If necessary, try to move the dependency to `config`.
  * For example, prefer `getInitialPositionFromUrl(url: string)` and call it as `getInitialPositionFromUrl(location.href)` instead of assuming that `location` is a global object.

### Javascript and array operations

* Prefer to name filter and map functions rather than inlining them
  - Do:
  ```
  const notNull = (_) => !!_
  return myArray.filter(notNull)
  ```
  - Instead of:
  ```
  return myArray.filter((_) => !!_)
  ```
* Prefer `array.includes(_)` instead of `array.indexOf(_) === -1`
* Prefer `str.startsWith(_)` instead of `str.indexOf(_) === 0`
* Prefer `CONSTANT.length` instead of `9 // length of "my string"`

### Development guidelines

* If you find code that is not used anywhere, delete it.
