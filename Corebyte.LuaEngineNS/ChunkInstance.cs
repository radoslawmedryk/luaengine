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

        public bool IsPaused { get; private set; }
        public bool IsAlive { get; private set; }

        #endregion

        #region Constructors

        internal ChunkInstance(LuaEngine luaEngine)
        {
            LuaEngine = luaEngine;

            // TODO: Store information about the internal Lua coroutine-based instance here
        }

        #endregion

        #region Methods

        public void PauseExecution(bool waitForPaused)
        {
            if (IsPaused)
                return;

            //TODO: Notify Lua core that we wish to puase execution of this instance
            throw new NotImplementedException();
        }

        public void ContinueExecution(bool waitForContinue)
        {
            if (!IsPaused)
                return;

            //TODO: Notify Lua core that we wish to continue execution of this instance
            throw new NotImplementedException();
        }

        public void StopExecution(bool waitForStop)
        {
            if (!IsAlive)
                return;

            //TODO: Notify Lua core that we wish to kill execution of this instance
            throw new NotImplementedException();
        }

        #endregion
    }
}
