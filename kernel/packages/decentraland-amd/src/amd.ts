type ModuleDescriptor = {
  rpcHandle: string
  methods: MethodDescriptor[]
}

type MethodDescriptor = { name: string }

type DecentralandInterface = {
  loadModule(moduleName: string): PromiseLike<ModuleDescriptor>
  callRpc(moduleHandle: string, methodName: string, args: ArrayLike<any>): PromiseLike<any>
  onStart(cb: Function)
}

type Module = {
  name: string
  dclamd: 1 | 2
  context: any
  dependencies?: string[]
  handlers: Function[]
  exports: any
}

declare var dcl: DecentralandInterface

// A naive attempt at getting the global `this`. Don’t use `this`!
const getGlobalThis = function () {
  // @ts-ignore
  if (typeof globalThis !== 'undefined') return globalThis
  // @ts-ignore
  if (typeof self !== 'undefined') return self
  // @ts-ignore
  if (typeof window !== 'undefined') return window
  // Note: this might still return the wrong result!
  if (typeof this !== 'undefined') return this
  throw new Error('Unable to locate global `this`')
}

const globalObject = (getGlobalThis as any)()

namespace loader {
  'use strict'

  const MODULE_LOADING = 1
  const MODULE_READY = 2

  let anonymousQueue = []

  const settings = {
    baseUrl: ''
  }

  const registeredModules: Record<string, Module> = {}

  export function config(config) {
    if (typeof config === 'object') {
      for (let x in config) {
        if (config.hasOwnProperty(x)) {
          settings[x] = config[x]
        }
      }
    }
  }

  function createModule(name: string, context?: any, handlers: Function[] = []): Module {
    return {
      name,
      dclamd: MODULE_LOADING,
      handlers,
      context,
      exports: {}
    }
  }

  export function define(factory: any)
  export function define(id: string, factory: any)
  export function define(id: string, dependencies: string[], factory: any)
  export function define(id: string, dependencies?, factory?) {
    let argCount = arguments.length

    if (argCount === 1) {
      factory = id
      dependencies = ['require', 'exports', 'module']
      id = null
    } else if (argCount === 2) {
      if (settings.toString.call(id) === '[object Array]') {
        factory = dependencies
        dependencies = id
        id = null
      } else {
        factory = dependencies
        dependencies = ['require', 'exports', 'module']
      }
    }

    if (!id) {
      anonymousQueue.push([dependencies, factory])
      return
    }

    function ready() {
      let handlers, context
      if (registeredModules[id]) {
        handlers = registeredModules[id].handlers
        context = registeredModules[id].context
      } else {
        registeredModules[id] = createModule(id)
      }

      let module = registeredModules[id]
      module.exports =
        typeof factory === 'function'
          ? factory.apply(null, anonymousQueue.slice.call(arguments, 0)) || registeredModules[id].exports || {}
          : factory

      module.dclamd = MODULE_READY
      module.context = context
      for (let x = 0, xl = handlers ? handlers.length : 0; x < xl; x++) {
        handlers[x](module.exports)
      }
    }

    if (!registeredModules[id]) registeredModules[id] = createModule(id)

    updateExistingModuleIfIndexModule(id)

    registeredModules[id].dependencies = dependencies.map((dep) => _toUrl(dep, id))

    require(dependencies, ready, id)
  }

  function updateExistingModuleIfIndexModule(id: string) {
    const idParts = id.split('/')

    if (idParts[idParts.length - 1] === 'index' && idParts.length > 1) {
      idParts.pop()
      const noIndexId = idParts.join('/')

      const existingModule = registeredModules[noIndexId]

      registeredModules[noIndexId] = registeredModules[id]

      if (existingModule) {
        registeredModules[noIndexId].context = existingModule.context
        registeredModules[noIndexId].handlers.push(...existingModule.handlers)
      }
    }
  }

  export namespace define {
    export const amd = {}
  }

  function hasDependencyWith(moduleId: string, otherModuleId: string): boolean {
    if (!registeredModules[moduleId] || !registeredModules[moduleId].dependencies) {
      return false
    }

    const directDependencies = registeredModules[moduleId].dependencies

    // Dependencies can be viewed as a graph. We keep track of the transitions between nodes that we have visited (to avoid infinite loop),
    // and those that we want to visit next. We want to find all the nodes reachable from moduleId, to see if those inclued otherModuleId
    const visited = [moduleId, 'require', 'exports', 'module']

    // If the dependency is one of those automatically assumed visited, we know we have a dependency
    if (visited.indexOf(otherModuleId) !== -1) return true

    const toVisit = directDependencies.filter((dep) => visited.indexOf(dep) === -1)

    while (toVisit.length > 0) {
      // If among the nodes we want to visit we have the module, then we know we have a dependency
      if (toVisit.indexOf(otherModuleId) !== -1) return true

      const dependencyId = toVisit.shift()

      visited.push(dependencyId)

      const module = registeredModules[dependencyId]
      if (module && module.dependencies) {
        for (let i = 0; i < module.dependencies.length; i++) {
          const moduleDependencyId = module.dependencies[i]
          if (visited.indexOf(moduleDependencyId) === -1 && toVisit.indexOf(moduleDependencyId) === -1) {
            toVisit.push(moduleDependencyId)
          }
        }
      }
    }

    // If we have visited all the graph and we didn't find a dependency, then as far as we know, we don't have a dependency yet
    return false
  }

  export function require(modules: string, callback?: Function, context?: string)
  export function require(modules: string[], callback?: Function, context?: string)
  export function require(modules: string | string[], callback?: Function, context?: string) {
    let loadedModulesExports: any[] = []
    let loadedCount = 0
    let hasLoaded = false

    if (typeof modules === 'string') {
      if (registeredModules[modules] && registeredModules[modules].dclamd === MODULE_READY) {
        return registeredModules[modules]
      }
      throw new Error(
        modules + ' has not been defined. Please include it as a dependency in ' + context + "'s define()"
      )
    }

    const xl = modules.length

    for (let x = 0; x < xl; x++) {
      switch (modules[x]) {
        case 'require':
          let _require: typeof require = function (new_module, callback) {
            return require(new_module, callback, context)
          } as any
          _require.toUrl = function (module) {
            return _toUrl(module, context)
          }
          loadedModulesExports[x] = _require
          loadedCount++
          break
        case 'exports':
          loadedModulesExports[x] = registeredModules[context].exports
          loadedCount++
          break
        case 'module':
          loadedModulesExports[x] = {
            id: context,
            uri: _toUrl(context)
          }
          loadedCount++
          break
        default:
          // If we have a circular dependency, then we resolve the module even if it hasn't loaded yet
          if (hasDependencyWith(modules[x], context)) {
            loadedModulesExports[x] = registeredModules[modules[x]].exports
            loadedCount++
          } else {
            load(
              modules[x],
              (loadedModuleExports) => {
                loadedModulesExports[x] = loadedModuleExports
                loadedCount++

                if (loadedCount === xl && callback) {
                  hasLoaded = true
                  callback.apply(null, loadedModulesExports)
                }
              },
              context
            )
          }
      }
    }

    if (!hasLoaded && loadedCount === xl && callback) {
      callback.apply(null, loadedModulesExports)
    }
  }

  function createMethodHandler(rpcHandle: string, method: MethodDescriptor) {
    return function () {
      return dcl.callRpc(rpcHandle, method.name, anonymousQueue.slice.call(arguments, 0))
    }
  }

  function load(moduleName: string, handler: Function, context: string) {
    moduleName = context ? _toUrl(moduleName, context) : moduleName

    if (registeredModules[moduleName]) {
      if (registeredModules[moduleName].dclamd === MODULE_LOADING) {
        handler && registeredModules[moduleName].handlers.push(handler)
      } else {
        handler && handler(registeredModules[moduleName].exports)
      }
    } else {
      const module = (registeredModules[moduleName] = createModule(moduleName, context, [handler]))
      if (moduleName.indexOf('@') === 0) {
        if (typeof dcl !== 'undefined') {
          dcl.loadModule(moduleName).then((descriptor: ModuleDescriptor) => {
            let createdModuleExports = {}

            for (let i in descriptor.methods) {
              const method = descriptor.methods[i]
              createdModuleExports[method.name] = createMethodHandler(descriptor.rpcHandle, method)
            }

            // This is somewhat repeated with the ready function above... Should refactor with clear head
            module.dclamd = MODULE_READY
            module.exports = createdModuleExports

            const handlers = module.handlers

            for (let i = 0; i < handlers.length; i++) {
              handlers[i](createdModuleExports)
            }
          })
        }
      }
    }
  }

  if (typeof dcl !== 'undefined') {
    dcl.onStart(() => {
      const notLoadedModules: Module[] = []
      for (let i in registeredModules) {
        if (registeredModules[i] && registeredModules[i].dclamd === MODULE_LOADING) {
          notLoadedModules.push(registeredModules[i])
        }
      }

      if (notLoadedModules.length) {
        throw new Error(`These modules didn't load: ${notLoadedModules.map(($) => $.name).join(', ')}`)
      }
    })
  }

  function _toUrl(id: string, context?: string) {
    let changed = false
    switch (id) {
      case 'require':
      case 'exports':
      case 'module':
        return id
    }
    const newContext = (context || settings.baseUrl).split('/')
    newContext.pop()
    const idParts = id.split('/')
    let i = idParts.length
    while (--i) {
      switch (idParts[0]) {
        case '..':
          newContext.pop()
        case '.':
        case '':
          idParts.shift()
          changed = true
      }
    }
    return (newContext.length && changed ? newContext.join('/') + '/' : '') + idParts.join('/')
  }

  require.toUrl = _toUrl
}

globalObject.define = loader.define
globalObject.dclamd = loader
