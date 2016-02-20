using NLua;
using System;
using System.Collections.Generic;
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

        private Lua Lua { get; set; }
        private Thread LuaThread { get; set; }
        private LuaFunctions LuaFunctions { get; set; }

        private object communicationLock = new object();

        private Dictionary<int, CompiledChunk> CompiledChunks { get; set; }
        private Dictionary<int, ChunkInstance> ChunkInstances { get; set; }

        private Random RNG { get; set; }

        #endregion

        #region Constructors

        public LuaEngine()
        {
            Lua = new Lua();
            LuaThread = new Thread(CoroutinesWatch);

            CompiledChunks = new Dictionary<int, CompiledChunk>();
            ChunkInstances = new Dictionary<int, ChunkInstance>();

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

        public CompiledChunk CompileChunk(String chunk)
        {
            throw new NotImplementedException();
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
            } while (ChunkInstances.ContainsKey(current));

            return current;
        }

        internal void ExchangeMessages()
        {
            throw new NotImplementedException();
        }
        
        private void CoroutinesWatch()
        {
            var _coroutinesWatch = Lua.GetFunction("_coroutinesWatch");
            _coroutinesWatch.Call(); // _coroutinesWatch have ininite loop inside Lua core.
            // exits only on LuaEngine shutdown.
        }

        #endregion

        #endregion
    }
}
