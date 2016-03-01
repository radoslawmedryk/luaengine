using System;

namespace Corebyte.LuaEngineNS
{
    public class LuaEngineNotRunningException : InvalidOperationException
    {
        public LuaEngineNotRunningException()
            : base()
        { }

        public LuaEngineNotRunningException(String message)
            : base(message)
        { }

        public LuaEngineNotRunningException(String message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
