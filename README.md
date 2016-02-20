# luaengine

LUAENGINE IS NOT YET FINISHED NOR APPLICABLE. THIS GITHUB ENTRY IS NOW USED ONLY FOR DEVELOPMENT USE ONLY.

LuaEngine is a C# project featuring an Lua engine capable of:
- managing the compilation of Lua scripts, giving the user information if any compilation-time error occured in the script;
- running every script in a secure Lua Sandbox, denying the user from disturbing the work of the Sandbox/OS;
- allowing a managed execution of multiple scripts/instances of script on the same time, concurrently;
- giving the user control over the execution of scripts, by allowing him to pause/stop any instance of any running script;
- allowing the user to control how many instances of any given script can be running at the same time;

What was the main goal when creating LuaEngine?
LuaEngine is perfectly fited for use in applications that needs to give the end user functionality to create small (or even bigger ones) pieces of Lua scripts that will control diffrent functionalities of the application. LuaEngine offers safe LuaSandbox and management of executed scripts out-of-the-box.

### Examples of LuaEngine usage
```C#
LuaEngine engine = new LuaEngine();
// print function is by default printing to the console.
// wait is internal LuaEngine function allowing pause in execution of the script for given ammount of time,
// without stoping other running Lua instances.
String script = "for i=1,5 do print(i) wait(1000) end";
CompiledChunk chunk = engine.CompileChunk(script);

ChunkInstance i1 = chunk.ExecuteChunk();
ChunkInstance i2 = chunk.ExecuteChunk();
ChunkInstance i3 = chunk.ExecuteChunk();

Thread.Sleep(1500);

i2.StopExecution();
i3.PauseExecution();

Thread.Sleep(2000);

i3.ContinueExecution();
Console.ReadLine();

// Produced output:
// 1
// 1
// 1
// 2
// 2
// 2
// 3
// 4
// 5
// 3
// 4
// 5
```
