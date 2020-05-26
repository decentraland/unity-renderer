type ModuleDescriptor = {
  rpcHandle: string;
  methods: MethodDescriptor[];
};

type MethodDescriptor = { name: string };

type DecentralandInterface = {
  loadModule(moduleName: string): PromiseLike<ModuleDescriptor>;
  callRpc(moduleHandle: string, methodName: string, args: ArrayLike<any>): PromiseLike<any>;
  onStart(cb: Function);
};

type Module = {
  name: string;
  dclamd: 1 | 2;
  context: any;
  handlers: Function[];
};

declare var dcl: DecentralandInterface;
declare var global: any;
declare var self: any;

namespace loader {
  "use strict";

  const MODULE_LOADING = 1;
  const MODULE_READY = 2;

  let anonymousQueue = [];

  const settings = {
    baseUrl: ""
  };

  const registeredModules: Record<string, Module> = {};

  export function config(config) {
    if (typeof config === "object") {
      for (let x in config) {
        if (config.hasOwnProperty(x)) {
          settings[x] = config[x];
        }
      }
    }
  }

  export function define(factory: any);
  export function define(id: string, factory: any);
  export function define(id: string, dependencies: string[], factory: any);
  export function define(id: string, dependencies?, factory?) {
    let argCount = arguments.length;

    if (argCount === 1) {
      factory = id;
      dependencies = ["require", "exports", "module"];
      id = null;
    } else if (argCount === 2) {
      if (settings.toString.call(id) === "[object Array]") {
        factory = dependencies;
        dependencies = id;
        id = null;
      } else {
        factory = dependencies;
        dependencies = ["require", "exports", "module"];
      }
    }

    if (!id) {
      anonymousQueue.push([dependencies, factory]);
      return;
    }

    function ready() {
      let handlers, context;
      if (registeredModules[id]) {
        handlers = registeredModules[id].handlers;
        context = registeredModules[id].context;
      }
      let module = (registeredModules[id] =
        typeof factory === "function"
          ? factory.apply(null, anonymousQueue.slice.call(arguments, 0)) || registeredModules[id] || {}
          : factory);
      module.dclamd = MODULE_READY;
      module.context = context;
      for (let x = 0, xl = handlers ? handlers.length : 0; x < xl; x++) {
        handlers[x](module);
      }
    }

    require(dependencies, ready, id);
  }

  export namespace define {
    export const amd = {};
  }

  export function require(modules: string, callback?: Function, context?: string);
  export function require(modules: string[], callback?: Function, context?: string);
  export function require(modules: string | string[], callback?: Function, context?: string) {
    let loadedModules: any[] = [];
    let loadedCount = 0;
    let hasLoaded = false;

    if (typeof modules === "string") {
      if (registeredModules[modules] && registeredModules[modules].dclamd === MODULE_READY) {
        return registeredModules[modules];
      }
      throw new Error(
        modules + " has not been defined. Please include it as a dependency in " + context + "'s define()"
      );
    }

    const xl = modules.length;

    for (let x = 0; x < xl; x++) {
      switch (modules[x]) {
        case "require":
          let _require: typeof require = function(new_module, callback) {
            return require(new_module, callback, context);
          } as any;
          _require.toUrl = function(module) {
            return toUrl(module, context);
          };
          loadedModules[x] = _require;
          loadedCount++;
          break;
        case "exports":
          loadedModules[x] = registeredModules[context] || (registeredModules[context] = {} as any);
          loadedCount++;
          break;
        case "module":
          loadedModules[x] = {
            id: context,
            uri: toUrl(context)
          };
          loadedCount++;
          break;
        case registeredModules[context] ? registeredModules[context].context : "":
          loadedModules[x] = registeredModules[registeredModules[context].context];
          loadedCount++;
          break;
        default:
          load(
            modules[x],
            loadedModuleExports => {
              loadedModules[x] = loadedModuleExports;
              loadedCount++;
              if (loadedCount === xl && callback) {
                hasLoaded = true;
                callback.apply(null, loadedModules);
              }
              if (registeredModules[modules[x]]) {
                registeredModules[modules[x]].dclamd = MODULE_READY;
              }
            },
            context
          );
      }
    }

    if (!hasLoaded && loadedCount === xl && callback) {
      callback.apply(null, loadedModules);
    }
  }

  function createMethodHandler(rpcHandle: string, method: MethodDescriptor) {
    return function() {
      return dcl.callRpc(rpcHandle, method.name, anonymousQueue.slice.call(arguments, 0));
    };
  }

  function load(moduleName: string, callback: Function, context: string) {
    moduleName = context ? toUrl(moduleName, context) : moduleName;

    if (registeredModules[moduleName]) {
      if (registeredModules[moduleName].dclamd === MODULE_LOADING) {
        callback && registeredModules[moduleName].handlers.push(callback);
      } else {
        callback && callback(registeredModules[moduleName]);
      }
      return;
    } else {
      registeredModules[moduleName] = {
        name: moduleName,
        dclamd: MODULE_LOADING,
        handlers: [callback],
        context: context
      };
    }

    if (moduleName.indexOf("@") === 0) {
      if (typeof dcl !== "undefined") {
        dcl.loadModule(moduleName).then((descriptor: ModuleDescriptor) => {
          let createdModule = {};

          for (let i in descriptor.methods) {
            const method = descriptor.methods[i];
            createdModule[method.name] = createMethodHandler(descriptor.rpcHandle, method);
          }

          callback(createdModule);
        });
      }
    }
  }

  if (typeof dcl !== "undefined") {
    dcl.onStart(() => {
      const notLoadedModules: Module[] = [];
      for (let i in registeredModules) {
        if (registeredModules[i] && registeredModules[i].dclamd === MODULE_LOADING) {
          notLoadedModules.push(registeredModules[i]);
        }
      }

      if (notLoadedModules.length) {
        throw new Error(`These modules didn't load: ${notLoadedModules.map($ => $.name).join(", ")}`);
      }
    });
  }

  function toUrl(id: string, context?: string) {
    let changed = false;
    switch (id) {
      case "require":
      case "exports":
      case "module":
        return id;
    }
    const newContext = (context || settings.baseUrl).split("/");
    newContext.pop();
    const idParts = id.split("/");
    let i = idParts.length;
    while (--i) {
      switch (id[0]) {
        case "..":
          newContext.pop();
        case ".":
        case "":
          idParts.shift();
          changed = true;
      }
    }
    return (newContext.length && changed ? newContext.join("/") + "/" : "") + idParts.join("/");
  }

  require.toUrl = toUrl;
}

global =
  typeof global !== "undefined"
    ? global
    : typeof self !== "undefined"
      ? self
      : typeof this !== "undefined"
        ? this
        : null;

if (!global) throw new Error("unknown global context");

global.define = loader.define;
global.dclamd = loader;
