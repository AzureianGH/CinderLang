using BackendInterface;

namespace CCBBackend
{
    public sealed class CCBType : IType
    {
        private readonly string _mangleName;
        private readonly string _ccbName;

        public TypeKind Kind { get; }

        public CCBType? PointeeType { get; }
        public CCBType? ReturnType { get; }
        public IReadOnlyList<CCBType> ParameterTypes { get; } = Array.Empty<CCBType>();
        public bool IsVarArg { get; }

        private CCBType(TypeKind kind, string mangleName, string ccbName)
        {
            Kind = kind;
            _mangleName = mangleName;
            _ccbName = ccbName;
        }

        private CCBType(CCBType pointee)
            : this(TypeKind.PointerTypeKind, pointee + "*", "ptr")
        {
            PointeeType = pointee;
        }

        private CCBType(CCBType returnType, IReadOnlyList<CCBType> parameters, bool isVarArg)
            : this(TypeKind.FunctionTypeKind, "fn", "fn")
        {
            ReturnType = returnType;
            ParameterTypes = parameters;
            IsVarArg = isVarArg;
        }

        public static CCBType Int32 { get; } = new(TypeKind.IntegerTypeKind, "i32", "i32");
        public static CCBType Int8 { get; } = new(TypeKind.IntegerTypeKind, "i8", "i8");
        public static CCBType Float { get; } = new(TypeKind.FloatTypeKind, "float", "f32");
        public static CCBType Double { get; } = new(TypeKind.DoubleTypeKind, "double", "f64");
        public static CCBType Void { get; } = new(TypeKind.VoidTypeKind, "void", "void");

        public static CCBType CreatePointer(IType pointee)
        {
            if (pointee is not CCBType ccbPointee)
            {
                throw new NotImplementedException("Unsupported type implementation.");
            }

            return new CCBType(ccbPointee);
        }

        public static CCBType CreateFunction(IType returnType, IType[] parameters, bool isVarArg)
        {
            if (returnType is not CCBType ccbReturnType)
            {
                throw new NotImplementedException("Unsupported return type implementation.");
            }

            var ccbParameters = parameters.Select(AsCCB).ToArray();
            return new CCBType(ccbReturnType, ccbParameters, isVarArg);
        }

        public string ToCcbName() => _ccbName;

        public static CCBType AsCCB(IType type)
        {
            if (type is not CCBType ccbType)
            {
                throw new NotImplementedException("Unsupported type implementation.");
            }

            return ccbType;
        }

        public bool Equals(IType? other)
        {
            if (other is not CCBType o)
            {
                return false;
            }

            return Kind == o.Kind && _mangleName == o._mangleName && IsVarArg == o.IsVarArg;
        }

        public override bool Equals(object? obj)
            => obj is IType type && Equals(type);

        public override int GetHashCode()
            => HashCode.Combine((int)Kind, _mangleName, IsVarArg);

        public override string ToString() => _mangleName;
    }
}
