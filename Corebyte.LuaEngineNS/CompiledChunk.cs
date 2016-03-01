using System;
using System.Threading;

namespace Corebyte.LuaEngineNS
{
    public class CompiledChunk : IDisposable
    {
        #region Variables

        public LuaEngine LuaEngine { get; private set; }
        public String LuaCodeText { get; private set; }
        public String DebugName { get; private set; }

        public CompilationStatus CompilationStatus { get; internal set; }
        public LuaError LuaError { get; internal set; }

        public int ChunkID { get; private set; }
        internal bool IsAlive { get; private set; }

        private EventWaitHandle CompilationFinishedWaitHandle { get; set; }

        #endregion

        #region Constructors

        internal CompiledChunk(LuaEngine luaEngine, String luaCodeText)
        {
            LuaEngine = luaEngine;
            LuaCodeText = luaCodeText;

            IsAlive = true;
            ChunkID = luaEngine.GetFreeChunkID();

            CompilationFinishedWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        }

        #endregion

        #region Methods

        public ChunkInstance Execute()
        {
            if (LuaEngine.IsEngineDead)
                throw new LuaEngineNotRunningException();

            if (!IsAlive)
                throw new InvalidOperationException("Cannot execute this instance of CompiledChunk - it is dead. Have you disposed it?");

            // Notify Lua core that we wish to execute this chunk
            // (add it to scripts queue)
            return LuaEngine.QueueChunkExecution(this);
        }

        public void WaitForCompilation()
        {
            CompilationFinishedWaitHandle.WaitOne();
        }

        internal void NotifyCompilationFinished()
        {
            CompilationFinishedWaitHandle.Set();
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    IsAlive = false;
                    LuaEngine.NotifyDeadCompiledChunk(this);
                    CompilationFinishedWaitHandle.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
