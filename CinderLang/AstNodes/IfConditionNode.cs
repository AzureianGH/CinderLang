using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BackendInterface;

namespace CinderLang.AstNodes
{
    public class IfConditionNode : IAstConditionNode
    {
        public string Name { get; set; }
        public IAstNode[] Children { get; set; }
        public IBlock Then { get; set; }
        public IBlock Else { get; set; }
        public IBlock ContinueBlock { get; set; }
        public IAstContainerNode Parent { get; set; }

        public List<(IType,string,IValue)> ContextVariables { get; set; } = new();

        public void Generate(IAstNode parent)
        {
            if (parent is NameSpaceNode) ErrorManager.Throw(ErrorType.Syntax,"If statement cannot be nested inside a namepsace");
            else if (parent is IAstContainerNode container)
            {
                Parent = container;

                var statement = GenerationHelpers.ParseValue(Name,Program.Builder.Int1Type,container);

                var iid = GenerateRandomString(16);

                Then = MethodNode.CurrentMethod.Function.AppendBasicBlock(iid + ".True");
                Else = MethodNode.CurrentMethod.Function.AppendBasicBlock(iid + ".False");
                ContinueBlock = MethodNode.CurrentMethod.Function.AppendBasicBlock(iid + ".Then");

                var br = Program.Builder.BuildCondBr(statement, Then, Else);

                if (parent is IAstConditionNode cn) cn.ContinueBlock = ContinueBlock;
                else MethodNode.CurrentMethod.Alignment = ContinueBlock;

                Console.WriteLine(ContinueBlock.ToString());

                foreach (var item in Children)
                {
                    Program.Builder.PositionAtEnd(Then);
                    item.Generate(this);
                }

                Console.WriteLine(ContinueBlock.ToString());

                Program.Builder.PositionAtEnd(Then);
                Program.Builder.BuildBr(ContinueBlock);

                Program.Builder.PositionAtEnd(Else);
                Program.Builder.BuildBr(ContinueBlock);
            }
            else ErrorManager.Throw(ErrorType.Syntax, "If statement must be nested.");
        }

        static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return RandomNumberGenerator.GetString(chars, length);
        }
    }
}
