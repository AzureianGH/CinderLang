using BackendInterface;

namespace CCBBackend
{
    internal enum CCBValueKind
    {
        Invalid,
        Function,
        GlobalAddress,
        LocalAddress,
        ParameterValue,
        ConstantInt,
        ConstantReal,
        StackValue,
    }

    internal class CCBValue : IValue
    {
        private static int _nextHandle = 1;
        private CCBValue? _initializer;

        public int Handle { get; }
        public IType TypeOf { get; }
        internal CCBValueKind Kind { get; }
        internal object? Payload { get; }

        private CCBValue(IType type, CCBValueKind kind, object? payload, bool valid)
        {
            TypeOf = type;
            Kind = kind;
            Payload = payload;
            Handle = valid ? _nextHandle++ : 0;
        }

        public static CCBValue Invalid()
            => new(CCBType.Void, CCBValueKind.Invalid, null, false);

        public static CCBValue Function(CCBFunction fn)
            => new(fn.Signature, CCBValueKind.Function, fn, true);

        public static CCBValue GlobalAddress(CCBGlobal global)
            => new(CCBType.CreatePointer(global.Type), CCBValueKind.GlobalAddress, global, true);

        public static CCBValue LocalAddress(CCBFunction fn, int index, CCBType type)
            => new(CCBType.CreatePointer(type), CCBValueKind.LocalAddress, new CCBLocalRef(fn, index, type), true);

        public static CCBValue Parameter(CCBFunction fn, int index, CCBType type)
            => new(type, CCBValueKind.ParameterValue, new CCBParamRef(fn, index, type), true);

        public static CCBValue ConstantInt(CCBType type, ulong value, bool signedExt)
            => new(type, CCBValueKind.ConstantInt, new CCBConstInt(value, signedExt), true);

        public static CCBValue ConstantReal(CCBType type, double value)
            => new(type, CCBValueKind.ConstantReal, value, true);

        public static CCBValue Stack(CCBType type)
            => new(type, CCBValueKind.StackValue, null, true);

        public IValue Initializer
        {
            get
            {
                return _initializer ?? Invalid();
            }
            set
            {
                if (Kind != CCBValueKind.GlobalAddress)
                {
                    throw new InvalidOperationException("Only globals may have initializers.");
                }

                if (value is not CCBValue ccbValue)
                {
                    throw new NotImplementedException("Unsupported value implementation.");
                }

                var global = (CCBGlobal)Payload!;
                global.Initializer = ccbValue;
                _initializer = ccbValue;
            }
        }

        public IBlock AppendBasicBlock(string name)
        {
            if (Kind != CCBValueKind.Function)
            {
                throw new InvalidOperationException("Only functions can create basic blocks.");
            }

            var fn = (CCBFunction)Payload!;
            return new CCBBlock(fn);
        }

        public IValue GetParam(uint p)
        {
            if (Kind != CCBValueKind.Function)
            {
                return Invalid();
            }

            var fn = (CCBFunction)Payload!;
            if (p >= fn.Parameters.Count)
            {
                return Invalid();
            }

            return fn.Parameters[(int)p];
        }

        public bool Equals(IType? other)
        {
            if (other is not CCBValue o)
            {
                return false;
            }

            return Handle == o.Handle;
        }

        public override bool Equals(object? obj)
            => obj is IType type && Equals(type);

        public override int GetHashCode()
            => Handle.GetHashCode();

        public override string ToString() => TypeOf.ToString() ?? "value";
    }

    internal sealed record CCBConstInt(ulong Value, bool SignedExt);
    internal sealed record CCBLocalRef(CCBFunction Function, int Index, CCBType ElementType);
    internal sealed record CCBParamRef(CCBFunction Function, int Index, CCBType Type);
}
