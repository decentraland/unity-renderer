# Style guidelines

## Redux

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