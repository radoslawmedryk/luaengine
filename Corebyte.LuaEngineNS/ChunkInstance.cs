using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corebyte.LuaEngineNS
{
    public class ChunkInstance
    {
        #region Variables

        public LuaEngine LuaEngine { get; private set; }
        public CompiledChunk Chunk { get; private set; }

        public LuaError LuaError { get; internal set; }
        public bool IsPaused { get; internal set; }
        public bool IsAlive { get; internal set; }

        // TODO: runtime Lua error here

        internal int InstanceID { get; private set; }

        #endregion

        #region Constructors

        internal ChunkInstance(LuaEngine luaEngine, CompiledChunk chunk)
        {
            LuaEngine = luaEngine;
            Chunk = chunk;
            InstanceID = luaEngine.GetFreeInstanceID();

            IsAlive = true;
        }

        #endregion

        #region Methods

        public void Pause(bool waitForPaused)
        {
            LuaEngine.InstanceExecutionAction(this, ExecutionControlAction.Pause);

            // TODO: waitForPaused
            throw new NotImplementedException();
        }

        public void Continue(bool waitForContinue)
        {
            LuaEngine.InstanceExecutionAction(this, ExecutionControlAction.Continue);

            // TODO: waitForContinue
            throw new NotImplementedException();
        }

        public void Stop(bool waitForStop)
        {
            if (!IsAlive)
                return;

            LuaEngine.InstanceExecutionAction(this, ExecutionControlAction.Terminate);

            // TODO: waitForStop
            throw new NotImplementedException();
        }

        #endregion
    }
}
