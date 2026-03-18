using BackendInterface;

namespace CCBBackend
{
    internal sealed class CCBBlock : IBlock
    {
        public CCBFunction Function { get; }

        public CCBBlock(CCBFunction function)
        {
            Function = function;
        }
    }
}
