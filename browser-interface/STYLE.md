# Style guidelines

# Code order

```
imports

local constants

main exports

non-exported functions
```
# Redux

* `actions.ts` must contain, for each action the system or the user triggers:
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
    export type CreateNewUserRequest = ReturnType<createNewUserRequest>
    ```
* `actions.ts` CAN NOT contain other exports other than a set of these three elements.
* On naming conventions for actions:
  - Please follow `Request`, `Success` and `Failure` for common HTTP or other RPC-style processes.

# redux-sagas

If there are three or more consecutive selects that can be merged into one, do it. Don't:
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
  const { a, b, c } = yield select(allInfo)
}
function allInfo(state) {
  return {
    a: aSelector(state),
    b: bSelector(state),
    c: cSelector(state),
  }
}
```

# module imports

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

# code reuse and libraries

- Prefer local code in `lib/` rather than included libraries (even if imported from `@dcl/*`)
  * For example, prefer `lib/math/Vector3` rather than `@dcl/ecs-math/ReadOnlyVector3`
  * Even for small functionality, such as `${x},${y}` to encode a parcel position, prefer using `lib/decentraland/parcels/positionToString`
- Prefer using `jsonFetch` instead of `fetch(...).json()`
  * This allows better readability unless you will explicitely handle error cases without `try { ... } catch { ... }` (for example, to handle HTTP return codes)

# async/await

- Always prefer `await Promise.all([a, b])` rather than `await a; await b;`.
  * The less turnaround, the better and faster the experience. This really compounds!

# testability

- "Inversion of control": Always prefer arguments rather than accesses to global objects.
  * If necessary, try to move the dependency to `config`.
  * For example, prefer `getInitialPositionFromUrl(url: string)` and call it as `getInitialPositionFromUrl(location.href)` instead of assuming that `location` is a global object.

# Development guidelines

* If you find code that is not used anywhere, delete it.