using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BackendInterface;

namespace CinderLang.AstNodes
{
    public class ElseConditionNode : IAstConditionNode
    {
        public string Name { get; set; }
        public IAstNode[] Children { get; set; }
        public IAstContainerNode Parent { get; set; }
        public IBlock ContinueBlock { get; set; }

        public IfConditionNode ifcond;

        public List<(IType,string,IValue)> ContextVariables { get; set; } = new();

        public void Generate(IAstNode parent)
        {
            if (parent is NameSpaceNode) ErrorManager.Throw(ErrorType.Syntax,"Else statement cannot be nested inside a namepsace");
            else if (parent is IAstContainerNode container)
            {
                Parent = container;

                var tidx = Array.IndexOf(container.Children,this);

                if (tidx == 0 || (container.Children[tidx-1] is not IfConditionNode cmp)) ErrorManager.Throw(ErrorType.Syntax, "Else statement must be preceaded by an if.");
                else
                {
                    cmp.Else.RemoveTerminator();

                    ifcond = cmp;
                    ContinueBlock = cmp.Else;

                    foreach (var item in Children)
                    {
                        Program.Builder.PositionAtHead(cmp.Else);
                        item.Generate(this);
                    }

                    Program.Builder.PositionAtEnd(ifcond.Else);
                    Program.Builder.BuildBr(ifcond.ContinueBlock);
                }
            }
            else ErrorManager.Throw(ErrorType.Syntax, "Else statement must be nested.");
        }
    }
}
