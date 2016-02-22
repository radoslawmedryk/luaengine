using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corebyte.LuaEngineNS
{
    public enum CompilationStatus
    {
        AwaitingCompilation,
        CompiledOK,
        CompileError,
    }

    public enum LuaErrorType
    {
        CompileTimeError,
        RunTimeError,
    }

    internal enum CompilationControlAction
    {
        CompileNew,
        RemoveCompiled,
    }

    internal enum ExecutionControlAction
    {
        Continue,
        Pause,
        Terminate,
    }
}
