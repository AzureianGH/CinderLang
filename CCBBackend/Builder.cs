using System.Globalization;
using BackendInterface;

namespace CCBBackend
{
    public sealed class Builder : IBuilder
    {
        private Module? _currentModule;
        private CCBFunction? _currentFunction;

        public IType Int32Type => CCBType.Int32;
        public IType FloatType => CCBType.Float;
        public IType DoubleType => CCBType.Double;
        public IType VoidType => CCBType.Void;
        public IType Int8Type => CCBType.Int8;

        public void EmitToFile(string path, IModule module)
        {
            if (module is not Module ccbModule)
            {
                throw new NotImplementedException("Unsupported module implementation.");
            }

            File.WriteAllText(path, ccbModule.PrintToString());
        }

        public IValue BuildCall(IType Ty, IValue Fn, IValue[] Args, string name = "")
        {
            EnsureFunctionContext();

            var functionType = CCBType.AsCCB(Ty);
            if (functionType.Kind != TypeKind.FunctionTypeKind)
            {
                throw new InvalidOperationException("BuildCall requires a function signature type.");
            }

            if (Fn is not CCBValue fnValue || fnValue.Kind != CCBValueKind.Function)
            {
                throw new InvalidOperationException("BuildCall currently supports direct function symbols only.");
            }

            foreach (var arg in Args)
            {
                EmitPush(arg);
            }

            var fn = (CCBFunction)fnValue.Payload!;
            var returnType = functionType.ReturnType ?? CCBType.Void;
            var argsSignature = string.Join(',', functionType.ParameterTypes.Select(t => t.ToCcbName()));

            _currentFunction!.Emit($"call {fn.Name} {returnType.ToCcbName()} ({argsSignature})");
            return returnType.Kind == TypeKind.VoidTypeKind ? CCBValue.Invalid() : CCBValue.Stack(returnType);
        }

        public IValue BuildStore(IValue val, IValue ptr)
        {
            EnsureFunctionContext();
            EmitPush(val);

            if (ptr is not CCBValue target)
            {
                throw new NotImplementedException("Unsupported value implementation.");
            }

            switch (target.Kind)
            {
                case CCBValueKind.LocalAddress:
                {
                    var local = (CCBLocalRef)target.Payload!;
                    _currentFunction!.Emit($"store_local {local.Index.ToString(CultureInfo.InvariantCulture)}");
                    return CCBValue.Invalid();
                }
                case CCBValueKind.GlobalAddress:
                {
                    var global = (CCBGlobal)target.Payload!;
                    _currentFunction!.Emit($"store_global {global.Name}");
                    return CCBValue.Invalid();
                }
                default:
                    throw new InvalidOperationException("Store target must be a local or global address.");
            }
        }

        public IValue BuildAlloca(IType t, string name = "")
        {
            EnsureFunctionContext();
            var localType = CCBType.AsCCB(t);
            int index = _currentFunction!.LocalTypes.Count;
            _currentFunction.LocalTypes.Add(localType);
            return CCBValue.LocalAddress(_currentFunction, index, localType);
        }

        public void PositionAtEnd(IBlock block)
        {
            if (block is not CCBBlock ccbBlock)
            {
                throw new NotImplementedException("Unsupported block implementation.");
            }

            _currentFunction = ccbBlock.Function;
        }

        public IValue BuildRet(IValue value)
        {
            EnsureFunctionContext();
            EmitPush(value);
            _currentFunction!.Emit("ret");
            return CCBValue.Invalid();
        }

        public IValue BuildVoidRet()
        {
            EnsureFunctionContext();
            _currentFunction!.Emit("ret void");
            return CCBValue.Invalid();
        }

        public IModule CreateModuleWithName(string name)
        {
            _currentFunction = null;
            _currentModule = new Module(name);
            return _currentModule;
        }

        public IValue BuildGlobalString(string str)
        {
            EnsureFunctionContext();
            _currentFunction!.Emit($"const_str \"{EscapeCString(str)}\"");
            return CCBValue.Stack(CCBType.CreatePointer(CCBType.Int8));
        }

        public IValue CreateConstInt(IType type, ulong value, bool signedext = false)
            => CCBValue.ConstantInt(CCBType.AsCCB(type), value, signedext);

        public IValue CreateConstReal(IType type, double value)
            => CCBValue.ConstantReal(CCBType.AsCCB(type), value);

        public IType CreatePointer(IType t, int space = 0)
            => CCBType.CreatePointer(t);

        public IType CreateFunction(IType returnt, IType[] Parameters, bool isVarArg = false)
            => CCBType.CreateFunction(returnt, Parameters, isVarArg);

        public IValue BuildLoad(IType t, IValue v, string name = "")
        {
            EnsureFunctionContext();

            if (v is not CCBValue value)
            {
                throw new NotImplementedException("Unsupported value implementation.");
            }

            var outType = CCBType.AsCCB(t);
            switch (value.Kind)
            {
                case CCBValueKind.LocalAddress:
                {
                    var local = (CCBLocalRef)value.Payload!;
                    _currentFunction!.Emit($"load_local {local.Index.ToString(CultureInfo.InvariantCulture)}");
                    return CCBValue.Stack(outType);
                }
                case CCBValueKind.GlobalAddress:
                {
                    var global = (CCBGlobal)value.Payload!;
                    _currentFunction!.Emit($"load_global {global.Name}");
                    return CCBValue.Stack(outType);
                }
                default:
                    throw new InvalidOperationException("Load source must be a local or global address.");
            }
        }

        public IValue BuildFAdd(IValue a, IValue b, string name = "") => EmitBinary("add", a, b);
        public IValue BuildAdd(IValue a, IValue b, string name = "") => EmitBinary("add", a, b);

        public IValue BuildFSub(IValue a, IValue b, string name = "") => EmitBinary("sub", a, b);
        public IValue BuildSub(IValue a, IValue b, string name = "") => EmitBinary("sub", a, b);

        public IValue BuildFMul(IValue a, IValue b, string name = "") => EmitBinary("mul", a, b);
        public IValue BuildMul(IValue a, IValue b, string name = "") => EmitBinary("mul", a, b);

        public IValue BuildFDiv(IValue a, IValue b, string name = "") => EmitBinary("div", a, b);
        public IValue BuildSDiv(IValue a, IValue b, string name = "") => EmitBinary("div", a, b);

        private IValue EmitBinary(string op, IValue a, IValue b)
        {
            EnsureFunctionContext();

            if (a is not CCBValue left || b is not CCBValue right)
            {
                throw new NotImplementedException("Unsupported value implementation.");
            }

            EmitPush(left);
            EmitPush(right);

            var resultType = CCBType.AsCCB(left.TypeOf);
            _currentFunction!.Emit($"binop {op} {resultType.ToCcbName()}");
            return CCBValue.Stack(resultType);
        }

        private void EmitPush(IValue value)
        {
            EnsureFunctionContext();

            if (value is not CCBValue ccbValue)
            {
                throw new NotImplementedException("Unsupported value implementation.");
            }

            switch (ccbValue.Kind)
            {
                case CCBValueKind.ParameterValue:
                {
                    var p = (CCBParamRef)ccbValue.Payload!;
                    _currentFunction!.Emit($"load_param {p.Index.ToString(CultureInfo.InvariantCulture)}");
                    break;
                }
                case CCBValueKind.ConstantInt:
                {
                    var ty = CCBType.AsCCB(ccbValue.TypeOf);
                    var ci = (CCBConstInt)ccbValue.Payload!;
                    if (ty.Kind == TypeKind.PointerTypeKind && ci.Value == 0)
                    {
                        _currentFunction!.Emit($"const {ty.ToCcbName()} null");
                    }
                    else
                    {
                        _currentFunction!.Emit($"const {ty.ToCcbName()} {ci.Value.ToString(CultureInfo.InvariantCulture)}");
                    }
                    break;
                }
                case CCBValueKind.ConstantReal:
                {
                    var ty = CCBType.AsCCB(ccbValue.TypeOf);
                    var real = (double)ccbValue.Payload!;
                    _currentFunction!.Emit($"const {ty.ToCcbName()} {real.ToString("R", CultureInfo.InvariantCulture)}");
                    break;
                }
                case CCBValueKind.LocalAddress:
                {
                    var local = (CCBLocalRef)ccbValue.Payload!;
                    _currentFunction!.Emit($"addr_local {local.Index.ToString(CultureInfo.InvariantCulture)}");
                    break;
                }
                case CCBValueKind.GlobalAddress:
                {
                    var global = (CCBGlobal)ccbValue.Payload!;
                    _currentFunction!.Emit($"addr_global {global.Name}");
                    break;
                }
                case CCBValueKind.StackValue:
                    break;
                case CCBValueKind.Invalid:
                    throw new InvalidOperationException("Cannot emit an invalid value.");
                default:
                    throw new InvalidOperationException($"Cannot push value kind {ccbValue.Kind}.");
            }
        }

        private void EnsureFunctionContext()
        {
            if (_currentFunction is null)
            {
                throw new InvalidOperationException("No active function context.");
            }
        }

        private static string EscapeCString(string value)
        {
            return value
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("\"", "\\\"", StringComparison.Ordinal)
                .Replace("\n", "\\n", StringComparison.Ordinal)
                .Replace("\r", "\\r", StringComparison.Ordinal)
                .Replace("\t", "\\t", StringComparison.Ordinal)
                .Replace("\0", "\\0", StringComparison.Ordinal);
        }
    }
}
