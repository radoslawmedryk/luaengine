using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corebyte.LuaEngineNS
{
    internal class CompilationControlEntity
    {
        #region Variables

        internal CompiledChunk Chunk { get; private set; }
        internal CompilationControlAction Action { get; set; }

        #endregion

        #region Constructors

        internal CompilationControlEntity(CompiledChunk chunk, CompilationControlAction action)
        {
            Chunk = chunk;
            Action = action;
        }

        #endregion

        #region Methods

        //

        #endregion
    }
}
