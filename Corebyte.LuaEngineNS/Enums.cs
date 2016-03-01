namespace Corebyte.LuaEngineNS
{
    // Some of the enum values are used by Lua core and are hardcoded as number values,
    // So enum values are explicitly set to ensure cohesion between LuaEngine and Lua core.

    public enum CompilationStatus
    {
        AwaitingCompilation = 0,
        CompiledOK = 1,
        CompileError = 2,
    }

    public enum ExecutionStatus
    {
        Running = 0,
        Paused = 1,
        Finished = 2,
        Terminated = 3,
    }

    public enum LuaErrorType
    {
        CompileTimeError = 0,
        RunTimeError = 1,
    }

    internal enum CompilationControlAction
    {
        CompileNew = 0,
        RemoveCompiled = 1,
    }

    internal enum ExecutionControlAction
    {
        Continue = 0,
        Pause = 1,
        Terminate = 2,
    }
}
