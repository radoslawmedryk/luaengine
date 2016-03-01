namespace Corebyte.LuaEngineNS
{
    internal class CompilationControlEntity
    {
        #region Variables

        public CompiledChunk Chunk { get; private set; }
        public CompilationControlAction Action { get; set; }
        public int ActionInt { get { return (int)Action; } } // Value used by Lua
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
