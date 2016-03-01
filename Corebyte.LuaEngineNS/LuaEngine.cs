using NLua;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Corebyte.LuaEngineNS
{
    public class LuaEngine : IDisposable
    {
        #region Variables

        public bool IsEngineDead { get; private set; }

        public long StopwatchTime
        {
            get { return LuaStopwatch.ElapsedMilliseconds; }
        }

        public ReadOnlyCollection<String> RegisteredFunctionsNames { get; private set; }
        private List<String> _RegisteredFunctionsNames { get; set; }

        private Lua Lua { get; set; }
        private Thread LuaThread { get; set; }
        private LuaFunctions LuaFunctions { get; set; }

        private Stopwatch LuaStopwatch { get; set; }

        private object communicationLock = new object();

        private Dictionary<int, CompiledChunk> CompiledChunks { get; set; }
        private Dictionary<int, ChunkInstance> RunningInstances { get; set; }

        private List<CompilationControlEntity> CompilationControlList { get; set; }
        private List<ExecutionQueueEntity> ExecutionQueue { get; set; }
        private List<ExecutionControlEntity> ExecutionControlList { get; set; }

        private Random RNG { get; set; }

        private EventWaitHandle LuaStopedWaitHandle { get; set; }

        #endregion

        #region Constructors

        public LuaEngine()
            : this(null)
        { }

        public LuaEngine(LuaFunctions registerFunctions)
        {
            Lua = new Lua();
            LuaThread = new Thread(CoroutinesWatch);

            LuaFunctions = (registerFunctions != null)? registerFunctions : new LuaFunctions();

            _RegisteredFunctionsNames = new List<string>();
            RegisteredFunctionsNames = new ReadOnlyCollection<string>(_RegisteredFunctionsNames);

            LuaStopwatch = new Stopwatch();
            LuaStopwatch.Start();

            CompiledChunks = new Dictionary<int, CompiledChunk>();
            RunningInstances = new Dictionary<int, ChunkInstance>();

            CompilationControlList = new List<CompilationControlEntity>();
            ExecutionQueue = new List<ExecutionQueueEntity>();
            ExecutionControlList = new List<ExecutionControlEntity>();

            RNG = new Random();
            LuaStopedWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

            RegisterLuaFunctions(LuaFunctions);
            StartEngine();
        }

        #endregion

        #region Methods

        #region PublicMethods

        public CompiledChunk CompileChunk(String luaCodeText)
        {
            if (IsEngineDead)
                throw new LuaEngineNotRunningException();

            lock (communicationLock)
            {
                // Create new CompiledChunk object, queue it for compilation to Lua core
                var chunk = new CompiledChunk(this, luaCodeText);
                chunk.CompilationStatus = CompilationStatus.AwaitingCompilation;
                CompiledChunks.Add(chunk.ChunkID, chunk);
                CompilationControlList.Add(
                    new CompilationControlEntity(chunk, CompilationControlAction.CompileNew));
                return chunk;
            }
        }

        public void ShutdownEngine(bool waitForShutdown)
        {
            IsEngineDead = true;

            if (waitForShutdown)
                LuaStopedWaitHandle.WaitOne();
        }

        #endregion

        #region NonPublicMethods

        internal int GetFreeChunkID()
        {
            /*if (!Monitor.IsEntered(communicationLock))
                throw new SynchronizationLockException();*/

            int current;
            do
            {
                current = RNG.Next(0, Int32.MaxValue);
            } while (CompiledChunks.ContainsKey(current));

            return current;
        }

        internal int GetFreeInstanceID()
        {
            /*if (!Monitor.IsEntered(communicationLock))
                throw new SynchronizationLockException();*/

            int current;
            do
            {
                current = RNG.Next(0, Int32.MaxValue);
            } while (RunningInstances.ContainsKey(current));

            return current;
        }

        internal void NotifyDeadCompiledChunk(CompiledChunk chunk)
        {
            lock (communicationLock)
            {
                // Remove dead chunk from CompiledChunks
                // Cleanup queues/lists, notify Lua core that chunk is dead
                // RunningInstances are NOT removed. They will run until finished or stoped.
                CompiledChunks.Remove(chunk.ChunkID);
                ExecutionQueue.RemoveAll(Q => Q.Chunk.ChunkID == chunk.ChunkID);

                var CC = CompilationControlList.FirstOrDefault(Q => Q.Chunk.ChunkID == chunk.ChunkID);
                if (CC != null)
                    CC.Action = CompilationControlAction.RemoveCompiled;
                else
                    CompilationControlList.Add(
                        new CompilationControlEntity(chunk, CompilationControlAction.RemoveCompiled));
            }
        }

        internal ChunkInstance QueueChunkExecution(CompiledChunk chunk)
        {
            lock (communicationLock)
            {
                ChunkInstance instance = new ChunkInstance(this, chunk);
                if (chunk.LuaError != null)
                {
                    // no queying chunk for execution is done, as the chunk is proven to have compile-time error
                    instance.LuaError = chunk.LuaError;
                    instance.Status = ExecutionStatus.Terminated;
                    return instance; // dead instance is returned
                }

                instance.Status = ExecutionStatus.Running;
                var EQE = new ExecutionQueueEntity(chunk, instance);
                ExecutionQueue.Add(EQE);
                RunningInstances.Add(instance.InstanceID, instance);
                return instance; // alive instance is returned
            }
        }

        internal void InstanceExecutionAction(ChunkInstance instance, ExecutionControlAction action)
        {
            lock (communicationLock)
            {
                if ((action == ExecutionControlAction.Pause && instance.Status != ExecutionStatus.Running) ||
                (action == ExecutionControlAction.Continue && instance.Status != ExecutionStatus.Paused) || 
                (action == ExecutionControlAction.Terminate && instance.Status != ExecutionStatus.Running && instance.Status != ExecutionStatus.Paused))
                {
                    throw new InvalidOperationException();
                }

                switch (action)
                {
                    case ExecutionControlAction.Continue:
                        instance.Status = ExecutionStatus.Running;
                        break;
                    case ExecutionControlAction.Pause:
                        instance.Status = ExecutionStatus.Paused;
                        break;
                    case ExecutionControlAction.Terminate:
                        instance.Status = ExecutionStatus.Terminated;
                        break;
                }

                var ECE = ExecutionControlList.FirstOrDefault(Q => Q.Instance.InstanceID == instance.InstanceID);
                if (ECE == null)
                    ExecutionControlList.Add(new ExecutionControlEntity(instance, action));
                else
                    ECE.Action = action;
            }
        }

        internal OutgoingMessage ExchangeMessages(LuaTable incoming)
        {
            lock (communicationLock)
            {
                if (incoming != null && !IsEngineDead)
                {
                    HandleChunkReports((LuaTable)incoming["chunkReports"]);
                    HandleInstanceReports((LuaTable)incoming["instanceReports"]);
                }

                var outgoing = OutgoingMessage.Instance;
                if (!IsEngineDead)
                {
                    outgoing.CopyCompilationControlList(CompilationControlList);
                    outgoing.CopyExecutionQueue(ExecutionQueue);
                    outgoing.CopyExecutionControlList(ExecutionControlList);

                    CompilationControlList.Clear();
                    ExecutionQueue.Clear();
                    ExecutionControlList.Clear();
                }
                else
                    outgoing.ShouldLuaTerminate = true;

                return outgoing;
            }
        }

        private void RegisterLuaFunctions(LuaFunctions luaFunctions)
        {
            if (luaFunctions == null)
                throw new ArgumentNullException();

            luaFunctions.LuaEngine = this;

            var LuaFunctionsMethods = luaFunctions.GetType().GetMethods(
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);
            foreach (var methodInfo in LuaFunctionsMethods)
            {
                var registerA = (RegisterLuaFunctionAttribute)methodInfo.GetCustomAttributes(typeof(RegisterLuaFunctionAttribute), true).FirstOrDefault();
                if (registerA != null)
                {
                    string functionName = registerA.FunctionName;
                    var internalA = (InternalLuaFunctionAttribute)methodInfo.GetCustomAttributes(typeof(InternalLuaFunctionAttribute), true).FirstOrDefault();
                    if (internalA == null)
                    {
                        Lua.RegisterFunction(functionName, luaFunctions, methodInfo); // for internal use by the LuaEngine
                        Lua.RegisterFunction("_CS_" + functionName, luaFunctions, methodInfo);
                        _RegisteredFunctionsNames.Add(functionName);
                    }
                    else
                        Lua.RegisterFunction(functionName, luaFunctions, methodInfo); // for internal use by the LuaEngine
                }
            }
        }

        private void StartEngine()
        {
            try
            {
                Lua.DoString(Properties.Resources.sandbox_env, "sandbox_env");
                Lua.DoString(Properties.Resources.coroutines, "coroutines");
                LuaThread.Start();
                while (!LuaThread.IsAlive) ;
            }
            catch
            {
                IsEngineDead = true;
                throw;
            }
        }

        private void HandleChunkReports(LuaTable chunksReports)
        {
            foreach (LuaTable line in chunksReports.Values)
            {
                var temp = line["chunkID"];
                var t = temp.GetType();
                int chunkID = (int)(double)line["chunkID"];
                var compilationStatus = (CompilationStatus)(int)(double)line["compilationStatus"];
                String compilationError = (String)line["compilationError"];

                if (!CompiledChunks.ContainsKey(chunkID))
                    continue; // chunk with this ID is dead, but Lua core didn't knew that yet

                var chunk = CompiledChunks[chunkID];
                chunk.CompilationStatus = compilationStatus;
                if (compilationStatus == CompilationStatus.CompileError)
                    chunk.LuaError = new LuaError(LuaErrorType.CompileTimeError, compilationError);

                line.Dispose();

                chunk.NotifyCompilationFinished();
            }
            chunksReports.Dispose();
        }

        private void HandleInstanceReports(LuaTable instancesReports)
        {
            foreach (LuaTable line in instancesReports.Values)
            {
                int instanceID = (int)(double)line["instanceID"];
                var newExecutionStatus = (ExecutionStatus)(int)(double)line["newExecutionStatus"];
                var executionError = (string)line["executionError"];

                if (!RunningInstances.ContainsKey(instanceID))
                    throw new InvalidOperationException();
                var instance = RunningInstances[instanceID];

                bool notifyStarted = false;
                bool notifyEnded = false;

                if (!instance.AlreadyNotifiedStarted)
                    notifyStarted = true;

                if (newExecutionStatus == ExecutionStatus.Finished || newExecutionStatus == ExecutionStatus.Terminated)
                {
                    notifyEnded = true;

                    instance.Status = newExecutionStatus;
                    RunningInstances.Remove(instance.InstanceID);

                    if (newExecutionStatus == ExecutionStatus.Terminated && !String.IsNullOrEmpty(executionError))
                        instance.LuaError = new LuaError(LuaErrorType.RunTimeError, executionError);
                    else if (newExecutionStatus == ExecutionStatus.Terminated && instance.Chunk.LuaError != null)
                        instance.LuaError = instance.Chunk.LuaError;
                }

                line.Dispose();

                if (notifyStarted)
                    instance.NotifyInstanceStarted();
                if (notifyEnded)
                    instance.NotifyInstanceEnded();
            }
            instancesReports.Dispose();
        }

        private void CoroutinesWatch()
        {
            var _coroutinesWatch = Lua.GetFunction("_coroutinesWatch");
            _coroutinesWatch.Call(); // _coroutinesWatch have ininite loop inside Lua core.

            // exits only on LuaEngine shutdown.
            IsEngineDead = true;
            LuaStopedWaitHandle.Set();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                ShutdownEngine(true);

                if (disposing)
                {
                    foreach (var C in CompiledChunks.Values)
                        C.Dispose();
                    foreach (var I in RunningInstances.Values)
                        I.Dispose();

                    Lua.Dispose();
                }

                disposedValue = true;
            }
        }

        // override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~LuaEngine() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion

        #endregion

        #endregion
    }
}