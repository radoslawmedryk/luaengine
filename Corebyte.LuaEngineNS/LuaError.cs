using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corebyte.LuaEngineNS
{
    public class LuaError
    {
        #region Variables

        public LuaErrorType ErrorType { get; private set; }
        public String ErrorMessage { get; private set; }

        #endregion

        #region Constructors

        internal LuaError(LuaErrorType errorType, String errorMessage)
        {
            ErrorType = errorType;
            ErrorMessage = errorMessage;
        }

        #endregion

        #region Methods

        //

        #endregion
    }
}
