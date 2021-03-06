﻿namespace Corebyte.LuaEngineNS
{
    internal class ExecutionControlEntity
    {
        #region Variables

        public ChunkInstance Instance { get; private set; }
        public ExecutionControlAction Action { get; set; }
        public int ActionInt { get { return (int)Action; } } // Value used by Lua

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
