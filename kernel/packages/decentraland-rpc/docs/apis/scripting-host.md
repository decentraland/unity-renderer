# ScriptingHost

ScriptingHost are the glue between an external System (that could run on a Web Worker, or even on another server) and a local API. By design each instance of a System is associated to an individual instance of a ScriptingHost and to an individual instance of the API it's trying to consume. This is done due to the need to support different Transport mechanisms (JSON-RPC over Worker messages, WebSockets or Memory), and to keep the state of each API isolated.

To summarize, a ScriptingHost serves the following purposes:

* Mounting and unmounting APIs on-demand.
* Providing communication over a given transport layer.
* Providing abstractions over notifications.

In practice, ScriptingHost are transparent and you will rarely find youself interacting with their API.
