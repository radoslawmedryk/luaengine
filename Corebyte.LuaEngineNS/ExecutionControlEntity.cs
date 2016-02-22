using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corebyte.LuaEngineNS
{
    internal class ExecutionControlEntity
    {
        #region Variables

        internal ChunkInstance Instance { get; private set; }
        internal ExecutionControlAction Action { get; set; }

        #endregion

        #region Constructors

        internal ExecutionControlEntity(ChunkInstance instance, ExecutionControlAction action)
        {
            Instance = instance;
            Action = action;
        }

        #endregion

        #region Methods

        //

        #endregion
    }
}
