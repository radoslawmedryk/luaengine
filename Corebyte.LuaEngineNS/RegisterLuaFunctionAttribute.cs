using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
