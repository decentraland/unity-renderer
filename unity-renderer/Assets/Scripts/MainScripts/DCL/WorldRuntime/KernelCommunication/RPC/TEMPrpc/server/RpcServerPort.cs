using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Google.Protobuf;

namespace rpc_csharp.server
{
    public class RpcServerPort<TContext>
    {
        private readonly Dictionary<string, UniTask<ServerModuleDeclaration>> loadedModules =
            new Dictionary<string, UniTask<ServerModuleDeclaration>>();

        private readonly Dictionary<uint, UnaryCallback<TContext>> procedures =
            new Dictionary<uint, UnaryCallback<TContext>>();

        private readonly Dictionary<uint, StreamCallback<TContext>> streamProcedures =
            new Dictionary<uint, StreamCallback<TContext>>();

        private readonly Dictionary<string, ModuleGeneratorFunction<TContext>> registeredModules =
            new Dictionary<string, ModuleGeneratorFunction<TContext>>();

        private uint proceduresCount = 0;

        public event Action OnClose;
        public uint portId { get; }
        public string portName { get; }

        public RpcServerPort(uint portId, string portName)
        {
            this.portId = portId;
            this.portName = portName;
        }

        public void Close()
        {
            loadedModules.Clear();
            procedures.Clear();
            streamProcedures.Clear();
            registeredModules.Clear();
            OnClose?.Invoke();
        }

        public void RegisterModule(string moduleName,
            ModuleGeneratorFunction<TContext> moduleDefinition)
        {
            if (registeredModules.ContainsKey(moduleName))
            {
                throw new Exception($"module ${moduleName} is already registered for port {portName} ({portId}))");
            }

            registeredModules.Add(moduleName, moduleDefinition);
        }

        public UniTask<ServerModuleDeclaration> LoadModule(string moduleName)
        {
            if (loadedModules.TryGetValue(moduleName, out UniTask<ServerModuleDeclaration> loadedModule))
            {
                return loadedModule;
            }

            if (!registeredModules.TryGetValue(moduleName, out ModuleGeneratorFunction<TContext> moduleGenerator))
            {
                throw new Exception($"Module ${moduleName} is not available for port {portName} ({portId}))");
            }

            var moduleFuture = LoadModuleFromGenerator(moduleGenerator(this));
            loadedModules.Add(moduleName, moduleFuture);

            return moduleFuture;
        }

        public async UniTask<(bool called, ByteString result)> TryCallUnaryProcedure(uint procedureId,
            ByteString payload, TContext context)
        {
            if (!procedures.TryGetValue(procedureId, out UnaryCallback<TContext> unaryCallback))
                return (called: false, result: null);

            var result = await unaryCallback(payload, context);
            return (called: true, result);
        }

        public bool TryCallStreamProcedure(uint procedureId, ByteString payload, TContext context,
            out IEnumerator<ByteString> result)
        {
            if (streamProcedures.TryGetValue(procedureId, out StreamCallback<TContext> streamProcedure))
            {
                result = streamProcedure(payload, context);
                return true;
            }

            result = default;
            return false;
        }

        private async UniTask<ServerModuleDeclaration> LoadModuleFromGenerator(
            UniTask<ServerModuleDefinition<TContext>> moduleFuture)
        {
            var module = await moduleFuture;

            var moduleUnaryDefinitions = module.definition;
            var moduleStreamDefinitions = module.streamDefinition;

            var ret = new ServerModuleDeclaration()
            {
                procedures =
                    new List<ServerModuleProcedureInfo>(moduleUnaryDefinitions.Count + moduleStreamDefinitions.Count)
            };

            using (var iterator = moduleUnaryDefinitions.GetEnumerator())
            {
                LoadProcedures(iterator, procedures, ret.procedures);
            }

            using (var iterator = moduleStreamDefinitions.GetEnumerator())
            {
                LoadProcedures(iterator, streamProcedures, ret.procedures);
            }

            return ret;
        }

        private void LoadProcedures<T>(IEnumerator<KeyValuePair<string, T>> procedureDefinition,
            IDictionary<uint, T> procedureMap, ICollection<ServerModuleProcedureInfo> infoList)
        {
            while (procedureDefinition.MoveNext())
            {
                var procedureId = proceduresCount++;
                var procedureName = procedureDefinition.Current.Key;
                procedureMap.Add(procedureId, procedureDefinition.Current.Value);

                infoList.Add(new ServerModuleProcedureInfo(procedureId, procedureName));
            }
        }
    }
}