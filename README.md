# luaengine

Version v0.1 of LuaEngine has been released. It is not yet well tested, so bugs are possible. Please report every bug you find to the author, thank you.

LuaEngine needs those libraries to run:
NLua: http://nlua.org/
Lua 5.2.3: http://www.lua.org/ftp/

LuaEngine is a C# project featuring an Lua engine capable of:
- managing the compilation of Lua scripts, giving the user information if any compilation-time error occured in the script;
- The most important functionality: adding wait(milis) function to Lua - it allows holding execution of one instance of script, while other instances are still working
- running every script in a secure Lua Sandbox, denying the user from disturbing the work of the Sandbox/OS;
- allowing a managed execution of multiple scripts/instances of script on the same time, concurrently;
- giving the user control over the execution of scripts, by allowing him to pause/stop any instance of any running script;

What was the main goal when creating LuaEngine?
LuaEngine is perfectly fited for use in applications that needs to give the end user functionality to create small (or even bigger ones) pieces of Lua scripts that will control diffrent functionalities of the application. LuaEngine offers safe LuaSandbox and management of executed scripts out-of-the-box.

LuaEngine itself is not a bridge library between C# and Lua - it depends on NLua project in this task (http://nlua.org). LuaEngine is a project working one level higher - managing compilation and execution of Lua scripts in a sandboxed, multithreading environment.

### Documentation
Documentation is not yet existing. It will change soon, though.

### Examples of LuaEngine usage

```C#
public class CustomLuaFunctions : LuaFunctions
{
    [RegisterLuaFunction("AddNumbers")]
    public double AddNumbers(double a, double b)
    {
        return a+b;
    }

    [RegisterLuaFunction("GetCSharpDog")]
    public Dog GetCSharpDog(String name)
    {
        return new Dog(name);
    }
}
```

```C#
public class Dog
{
    public String Name { get; private set; }

    public Dog(String name)
    {
        Name = name;
    }

    public void Bark()
    {
        Console.WriteLine(Name + ": Bark! Bark!");
    }
}
```

```C#
var customFunctions = new CustomLuaFunctions();
LuaEngine engine = new LuaEngine(customFunctions);

String chunkText1 = "print(AddNumbers(3,7))" + 
                    "wait(2500)" +
                    "print('message after wait')";
// Lua's print function by default prints to Console. It can be overridden.

// WARNING! CSharp objects exposed to Lua are NOT Sandboxed!
// NEVER expose them to end-users if you don't trust them,
// as they can execute almost ANY CSharp code by accessing just one
// CSharp object. If you need to expose data from CSharp to end-user
// write a wrapper around it in lua code not accessible by the end user.
String chunkText2 = "local dog = GetCSharpDog('Steve')" +
                    "dog:Bark()";

CompiledChunk chunk1 = engine.CompileChunk(chunkText1);
CompiledChunk chunk2 = engine.CompileChunk(chunkText2);

chunk1.WaitForCompilation();
chunk2.WaitForCompilation();
if (chunk1.LuaError != null || chunk2.LuaError != null)
{
    if (chunk1.LuaError != null)
        Console.WriteLine("chunk1 error: " + chunk1.LuaError.ErrorMessage);
    if (chunk2.LuaError != null)
        Console.WriteLine("chunk2 error: " + chunk2.LuaError.ErrorMessage);
    return;
}

ChunkInstance instance1 = chunk1.Execute();
Thread.Sleep(1500);
ChunkInstance instance2 = chunk2.Execute();

instance1.WaitForEnded();
instance2.WaitForEnded();
Console.WriteLine("All instances finished. Press enter to quit.");
Console.ReadLine();
instance1.Dispose(); instance2.Dispose(); chunk1.Dispose(); chunk2.Dispose();
engine.Dispose();
```
