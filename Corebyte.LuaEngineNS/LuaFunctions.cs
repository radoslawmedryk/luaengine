using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Corebyte.LuaEngineNS
{
    public class LuaFunctions
    {
        #region Variables

        internal LuaEngine LuaEngine { get; set; }

        #endregion

        #region Constructors

        internal protected LuaFunctions()
        {
            //
        }

        #endregion

        #region Methods
        
        [RegisterLuaFunction("_exchangeMessages")]
        [InternalLuaFunction]
        public Dictionary<String, object> _exchangeMessages(Dictionary<String, object> incoming)
        {
            throw new NotImplementedException();
        }

        [RegisterLuaFunction("_sleep")]
        public void _sleep(int time)
        {
            Thread.Sleep(time);
        }

        #endregion
    }
}
