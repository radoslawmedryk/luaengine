using System;

namespace Corebyte.LuaEngineNS
{
    public class RegisterLuaFunctionAttribute : Attribute
    {
        public String FunctionName { get; private set; }

        public RegisterLuaFunctionAttribute(String functionName)
        {
            FunctionName = functionName;
        }
    }
}
