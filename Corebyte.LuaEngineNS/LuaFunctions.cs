using NLua;
using System;
using System.Threading;

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
        internal OutgoingMessage _exchangeMessages(LuaTable incoming)
        {
            // TODO: improve this;
            // TEMP: Thread.Sleep to let other threads do more work and to decrease CPU usage
            Thread.Sleep(10);

            var outgoing = LuaEngine.ExchangeMessages(incoming);
            if (incoming != null)
                incoming.Dispose();

            return outgoing;
        }

        [RegisterLuaFunction("_sleep")]
        internal void _sleep(int time)
        {
            Thread.Sleep(time);
        }

        [RegisterLuaFunction("getTime")]
        internal long getTime()
        {
            return LuaEngine.StopwatchTime;
        }

        [RegisterLuaFunction("_print")]
        [InternalLuaFunction]
        internal void _print(object input)
        {
            Console.WriteLine(input != null ? input.ToString() : "nil");
        }

        #endregion
    }
}
