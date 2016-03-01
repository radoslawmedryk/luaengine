using System.Collections.Generic;

namespace Corebyte.LuaEngineNS
{
    internal class OutgoingMessage
    {
        #region Variables

        public static OutgoingMessage Instance { get; private set; }
            = new OutgoingMessage(); // Singleton due to performacne reasons

        public bool ShouldLuaTerminate { get; set; }

        // OutgoingMessage must contain a copy of containers, so LuaEngine is free
        // to clear/modify it's internal containers as soon, as they are copied to OutgoingMessage.
        // All properties of objects (inside those containers) used by Lua core are immutable, so only
        // shalow copy of every container is needed.
        public List<CompilationControlEntity> CompilationControlList { get; private set; }
        public List<ExecutionQueueEntity> ExecutionQueue { get; private set; }
        public List<ExecutionControlEntity> ExecutionControlList { get; private set; }

        #endregion

        #region Constructors

        private OutgoingMessage()
        {
            CompilationControlList = new List<CompilationControlEntity>();
            ExecutionQueue = new List<ExecutionQueueEntity>();
            ExecutionControlList = new List<ExecutionControlEntity>();
        }

        #endregion

        #region Methods

        internal void CopyCompilationControlList(List<CompilationControlEntity> source)
        {
            CompilationControlList.Clear();
            foreach (var I in source)
            {
                CompilationControlList.Add(I);
            }
        }

        internal void CopyExecutionQueue(List<ExecutionQueueEntity> source)
        {
            ExecutionQueue.Clear();
            foreach (var I in source)
            {
                ExecutionQueue.Add(I);
            }
        }

        internal void CopyExecutionControlList(List<ExecutionControlEntity> source)
        {
            ExecutionControlList.Clear();
            foreach (var I in source)
            {
                ExecutionControlList.Add(I);
            }
        }

        #endregion
    }
}