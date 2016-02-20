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
        public CompilationStatus CompilationStatus { get; private set; }
        public String ErrorMessage { get; private set; }

        internal int ChunkID { get; private set; }

        #endregion

        #region Constructors

        internal CompiledChunk(LuaEngine luaEngine)
        {
            LuaEngine = luaEngine;
            ChunkID = luaEngine.GetFreeChunkID();
            
            // TODO: Store a some kind of an internal instance to compiled chunk
        }

        #endregion

        #region Methods

        public ChunkInstance Execute()
        {
            // TODO: Notify Lua core that we wish to execute this chunk
            // (add it to scripts queue)
            throw new NotImplementedException();
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
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~CompiledChunk() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
