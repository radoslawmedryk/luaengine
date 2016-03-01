using System;
using System.Threading;

namespace Corebyte.LuaEngineNS
{
    public class ChunkInstance : IDisposable
    {
        #region Variables

        public LuaEngine LuaEngine { get; private set; }
        public CompiledChunk Chunk { get; private set; }

        public ExecutionStatus Status { get; internal set; }

        public int StatusInt { get { return (int)Status; } }
        public LuaError LuaError { get; internal set; }

        public int InstanceID { get; private set; }

        internal bool AlreadyNotifiedStarted { get; private set; }
        private EventWaitHandle InstanceStartedWaitHandle { get; set; }
        private EventWaitHandle InstanceEndedWaitHandle { get; set; }

        #endregion

        #region Constructors

        internal ChunkInstance(LuaEngine luaEngine, CompiledChunk chunk)
        {
            LuaEngine = luaEngine;
            Chunk = chunk;
            InstanceID = luaEngine.GetFreeInstanceID();

            InstanceStartedWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            InstanceEndedWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        }

        #endregion

        #region Methods

        public void Pause()
        {
            if (LuaEngine.IsEngineDead)
                throw new LuaEngineNotRunningException();

            LuaEngine.InstanceExecutionAction(this, ExecutionControlAction.Pause);
        }

        public void Continue()
        {
            if (LuaEngine.IsEngineDead)
                throw new LuaEngineNotRunningException();

            LuaEngine.InstanceExecutionAction(this, ExecutionControlAction.Continue);
        }

        public void Terminate()
        {
            if (LuaEngine.IsEngineDead)
                throw new LuaEngineNotRunningException();

            if (Status == ExecutionStatus.Terminated || Status == ExecutionStatus.Finished)
                return;

            LuaEngine.InstanceExecutionAction(this, ExecutionControlAction.Terminate);
        }

        public void WaitForStarted()
        {
            InstanceStartedWaitHandle.WaitOne();
            var a = 10;
        }

        public void WaitForEnded()
        {
            InstanceEndedWaitHandle.WaitOne();
            var a = 10;
        }

        internal void NotifyInstanceStarted()
        {
            if (AlreadyNotifiedStarted)
                return;

            InstanceStartedWaitHandle.Set();
            AlreadyNotifiedStarted = true;
        }

        internal void NotifyInstanceEnded()
        {
            InstanceEndedWaitHandle.Set();
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
                    InstanceStartedWaitHandle.Dispose();
                    InstanceEndedWaitHandle.Dispose();
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
