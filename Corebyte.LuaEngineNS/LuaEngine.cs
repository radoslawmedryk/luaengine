using NLua;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Corebyte.LuaEngineNS
{
    public class LuaEngine
    {
        #region Variables

        public bool IsEngineStarted { get; private set; }
        public bool IsEngineDead { get; private set; }

        public long StopwatchTime
        {
            get { return LuaStopwatch.ElapsedMilliseconds; }
        }

        private Lua Lua { get; set; }
        private Thread LuaThread { get; set; }
        private LuaFunctions LuaFunctions { get; set; }

        private Stopwatch LuaStopwatch { get; set; }

        private object communicationLock = new object();

        private Dictionary<int, CompiledChunk> CompiledChunks { get; set; }
        private Dictionary<int, ChunkInstance> RunningInstances { get; set; }

        private List<CompilationControlEntity> CompilationControlList { get; set; }
        private List<CompiledChunk> ExecutionQueue { get; set; }
        private List<ExecutionControlEntity> ExecutionControlList { get; set; }

        private Random RNG { get; set; }

        #endregion

        #region Constructors

        public LuaEngine()
        {
            Lua = new Lua();
            LuaThread = new Thread(CoroutinesWatch);

            LuaStopwatch = new Stopwatch();
            LuaStopwatch.Start();

            CompiledChunks = new Dictionary<int, CompiledChunk>();
            RunningInstances = new Dictionary<int, ChunkInstance>();

            CompilationControlList = new List<CompilationControlEntity>();
            ExecutionQueue = new List<CompiledChunk>();
            ExecutionControlList = new List<ExecutionControlEntity>();

            RNG = new Random();
            // TODO: constructor logic
        }

        #endregion

        #region Methods

        #region PublicMethods

        public void RegisterLuaFunctions(LuaFunctions luaFunctions)
        {
            if (luaFunctions == null)
                throw new ArgumentNullException();

            if (LuaFunctions != null)
                throw new InvalidOperationException("LuaFunctions for this LuaEngine instance are already registered.");

            LuaFunctions = luaFunctions;
            luaFunctions.LuaEngine = this;

            // TODO: registering functions code here
            throw new NotImplementedException();
        }

        public void StartEngine()
        {
            if (IsEngineStarted || IsEngineDead)
                throw new InvalidOperationException("LuaEngine cannot be started, becouse it was started before.");
            IsEngineStarted = true;

            if (LuaFunctions == null)// if user haven't registered custom LuaFunctions, register default one.
                RegisterLuaFunctions(new LuaFunctions());

            //LuaThread.Start();

            throw new NotImplementedException();
        }

        public void ShutdownEngine()
        {
            if (!IsEngineStarted || IsEngineDead)
                throw new InvalidOperationException("LuaEngine cannot be shuted down, becouse it is not started or is already dead.");
            IsEngineStarted = false;
            IsEngineDead = true;

            throw new NotImplementedException();
        }

        public CompiledChunk CompileChunk(String luaCodeText)
        {
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

        #endregion

        #region NonPublicMethods

        internal void Lock()
        {
            if (Monitor.IsEntered(communicationLock))
                throw new InvalidOperationException();

            Monitor.Enter(communicationLock);
        }

        internal void Unlock()
        {
            Monitor.Exit(communicationLock);
        }

        internal int GetFreeChunkID()
        {
            if (!Monitor.IsEntered(communicationLock))
                throw new SynchronizationLockException();

            int current;
            do
            {
                current = RNG.Next(0, Int32.MaxValue);
            } while (CompiledChunks.ContainsKey(current));

            return current;
        }

        internal int GetFreeInstanceID()
        {
            if (!Monitor.IsEntered(communicationLock))
                throw new SynchronizationLockException();

            int current;
            do
            {
                current = RNG.Next(0, Int32.MaxValue);
            } while (RunningInstances.ContainsKey(current));

            return current;
        }

        internal void NotifyDeadCompiledChunk(CompiledChunk chunk)
        {
            bool localLock = false;
            try
            {
                if (!Monitor.IsEntered(communicationLock)) // lock if not locked by current thread
                {
                    localLock = true;
                    Monitor.Enter(communicationLock);
                }

                // Remove dead chunk from CompiledChunks
                // Cleanup queues/lists, notify Lua core that chunk is dead
                // RunningInstances are NOT removed. They will run until finished or stoped.
                CompiledChunks.Remove(chunk.ChunkID);
                ExecutionQueue.RemoveAll(Q => Q.ChunkID == chunk.ChunkID);

                var CC = CompilationControlList.FirstOrDefault(Q => Q.Chunk.ChunkID == chunk.ChunkID);
                if (CC != null)
                    CC.Action = CompilationControlAction.RemoveCompiled;
                else
                    CompilationControlList.Add(
                        new CompilationControlEntity(chunk, CompilationControlAction.RemoveCompiled));
            }
            finally
            {
                if (localLock) // free lock if locked localy inside of this method
                    Monitor.Exit(communicationLock);
            }
        }

        internal ChunkInstance QueueChunkExecution(CompiledChunk chunk)
        {
            // if chunk have compilation error then set error on instance and return here
            ChunkInstance instance = new ChunkInstance(this, chunk);
            if (chunk.LuaError != null)
            {
                // no queying chunk for execution is done, as the chunk is proven to have compile-time error
                instance.LuaError = chunk.LuaError;
                instance.IsAlive = false;
                return instance; // dead instance is returned
            }

            bool localLock = false;
            try
            {
                if (!Monitor.IsEntered(communicationLock)) // lock if not locked by current thread
                {
                    localLock = true;
                    Monitor.Enter(communicationLock);
                }

                ExecutionQueue.Add(chunk);
                RunningInstances.Add(instance.InstanceID, instance);
                return instance; // alive instance is returned
            }
            finally
            {
                if (localLock) // free lock if locked localy inside of this method
                    Monitor.Exit(communicationLock);
            }
        }

        internal void InstanceExecutionAction(ChunkInstance instance, ExecutionControlAction action)
        {
            bool localLock = false;
            try
            {
                if (!Monitor.IsEntered(communicationLock)) // lock if not locked by current thread
                {
                    localLock = true;
                    Monitor.Enter(communicationLock);
                }

                ExecutionControlList.Add(
                    new ExecutionControlEntity(instance, action));
            }
            finally
            {
                if (localLock) // free lock if locked localy inside of this method
                    Monitor.Exit(communicationLock);
            }
        }

        internal List<OutgoingMessage> ExchangeMessages(List<IncomingMessage> incoming)
        {
            if (!Monitor.IsEntered(communicationLock))
                throw new SynchronizationLockException();

            throw new NotImplementedException();
        }
        
        private void CoroutinesWatch()
        {
            var _coroutinesWatch = Lua.GetFunction("_coroutinesWatch");
            _coroutinesWatch.Call(); // _coroutinesWatch have ininite loop inside Lua core.

            // exits only on LuaEngine shutdown.
            IsEngineStarted = false;
            IsEngineDead = true;
        }

        #endregion

        #endregion
    }
}
