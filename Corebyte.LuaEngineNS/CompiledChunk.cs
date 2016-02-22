using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corebyte.LuaEngineNS
{
    public class CompiledChunk : IDisposable
    {
        #region Variables

        public LuaEngine LuaEngine { get; private set; }
        public String LuaCodeText { get; private set; }

        public CompilationStatus CompilationStatus { get; internal set; }
        public LuaError LuaError { get; private set; }

        internal int ChunkID { get; private set; }
        internal bool IsAlive { get; private set; }
        internal LuaFunction ChunkFunction { get; private set; }

        #endregion

        #region Constructors

        internal CompiledChunk(LuaEngine luaEngine, String luaCodeText)
        {
            LuaEngine = luaEngine;
            LuaCodeText = luaCodeText;

            ChunkID = luaEngine.GetFreeChunkID();
        }

        #endregion

        #region Methods

        public ChunkInstance Execute()
        {
            if (!IsAlive)
                throw new InvalidOperationException("Cannot execute this instance of CompiledChunk - it is dead. Have you disposed it?");

            // Notify Lua core that we wish to execute this chunk
            // (add it to scripts queue)
            return LuaEngine.QueueChunkExecution(this);
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
