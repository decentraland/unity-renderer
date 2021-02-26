### Kernel-unity native interface explainer and maintenance guide

#### Overview 
In some cases, using `SendMessage` is not performant enough to pass data
from Kernel to the Unity Renderer. To address that we added a new native
bridge that ends up giving JS the ability to call C# methods directly.

The following files are involved

* *kernelNativeBridge.c / .h:* Here lies the glue between C# and JS. A
  few macros have been defined to define methods that will be exposed
  from JS and C# in order to pass the function pointers that we need to
  make the direct calls.

* *EntryPoint_World.cs:* Here, the `SetCallback` methods defined in
  `kernelNativeBridge` have to be called in order to have a C# method
  recognized and callable by JS.

* *nativeMessagesBridge.ts*: In this file we are initializing the
  JS-side call methods.

#### Step by step guide to add a new direct call function

* Add the function in the `kernelNativeBridge.c` file. This just
  involves adding `EXTERNAL_CALLBACK_<signature>(<method-name>)` line to
  the file. Note that the signature has to be specified.

* If the method
  has a signature not defined yet, you have to add a new macro that
  should use a new function pointer type declared in
  `kernelNativeBridge.h`. This macro will generate the
  `SetCallback_<method-name>` and `call<method-name>` functions used
  later.

* Bind the function in `EntryPoint_World.cs`. For this you will need the
  method signature delegate. Again, if the signature is new, you'll have
  to create a new one. When you have the delegate, you have to define a
  static method, looking like this:
  ```
    [MonoPInvokeCallback(typeof(JS_Delegate_VS))]
    private static void Foo(string id)
    {
        //your code
    }
  ```
  Remember, the method has to be static and has to use the
  `MonoPInvokeCallback` attribute with the proper delegate. Otherwise,
  compilation will fail and the methods will not be bound correctly.

* Put the `SetCallback_Foo` call in `World_EntryPoint` initialization
  with the method as a param. This will set the function pointer wasm
  side, finishing the "glue" between C# and JS.
  ```
    public EntryPoint_World(SceneController sceneController)
    {
        ...
        SetCallback_Foo(Foo);
  ```

*  Now you have to call the method. This example code would call the
   `Foo` method from JS. Remember that you need the `Module` instance
   that lies inside the unity instance JS side returned by the
   `UnityLoader` construct. Right now, this is already solved in
   `nativeMessagesBridge.ts`. The following example should add the `Foo`
   method to it.

   ```
   private callFoo!: (arg: string) => void
   ...
   public initNativeMessages(gameInstance: any) {
    ...
    this.callFoo = this.unityModule.cwrap('call_Foo', null, ['string'])
    ...
   }
   ```

   And later...

   ```
   this.callFoo("bar") // finally calling the function
   ```

* We did it!. Remember: this is no replacement of `SendMessage`.
  `SendMessage` is still used by our WSS debug mode. So, you'll have to
  always provide a `SendMessage` alternative and bind it correctly to
  our `WSSController`. From Kernel, you can branch between native and
  `SendMessage` calls using a snippet like this:
  ```
    if (WSS_ENABLED || FORCE_SEND_MESSAGE) {
      // SendMessage
    } else {
      // Native call
    }
  ```

* As you can see, binding native calls has a lot of maintenance costs,
  so its better to just bind calls that really need that performance
  speed up. If you need benchmarks, this approach is inspired by
  [this](https://forum.unity.com/threads/super-fast-javascript-interaction-on-webgl.382734/)
  unity forum post showing some numbers.
