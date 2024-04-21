using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CInterpreter
{
    internal class Executer : IExecuter
    {
        private IReadable read;
        private IWritable write;
        private int row;
        public Executer(InterpreterContext context, IReadable read, IWritable write)
        {
            this.context = context;
            this.read = read;
            this.write = write;
        }
        public bool ExecuteProgram(TreeNode node, int row)
        {
            this.row = row;
            if (node.Children.Count() == 1 && node.Children[0].Tocken is ParserTocken)
            {
                switch (((ParserTocken)node.Children[0].Tocken).parseTocken)
                {
                    case ParserTocken.ParserStages.DeclarationStatement:
                        return ExecuteDeclarationStatement(node.Children[0]);
                    case ParserTocken.ParserStages.InitialisationStatement:
                        return ExecuteInitialisationStatement(node.Children[0]);
                    case ParserTocken.ParserStages.ExpressionStatement:
                        return ExecuteExpressionStatement(node.Children[0]);
                }
            }
            
            return false;
        }

        bool ExecuteExpressionStatement(TreeNode node)
        {
            return ExecuteFunctionExpresion(node);
        }

        bool ExecuteDeclarationStatement(TreeNode node)
        {
            if (node.Children.Count() == 2)
            {
                KeyWordTocken? keyWord = node.Children[0].Tocken as KeyWordTocken;
                if (keyWord != null && keyWord.ID == KeyWordTocken.KeyWordID.Int)
                {
                    return ExecuteDeclarationList(node.Children[1].Children);
                }
            }
            return false;
        }

        bool ExecuteDeclarationList(List<TreeNode> declList)
        {
            foreach (TreeNode declaration in declList)
            {
                if (declaration.Children.Count() == 1)
                {
                    ChecIsVariableDeclare(declaration.Children[0]);
                }
                else
                {
                    int? value = ExecuteDigitExpresion(declaration.Children[2]);
                    if (value != null)
                        ChecIsVariableDeclare(declaration.Children[0], (int)value);
                    else
                        return false;
                }
            }
            return true;
        }

        bool ChecIsVariableDeclare(TreeNode node, int value = 0)
        {
            if (!intVars.ContainsKey(((IdentifierTocken)node.Tocken).ID))
            {
                intVars.Add(((IdentifierTocken)node.Tocken).ID, value);
                return true;
            }
            return false;
        }

        bool ExecuteInitialisationStatement(TreeNode node)
        {
            if (node.Children.Count() == 1)
            {
                ParserTocken? initList = node.Children[0].Tocken as ParserTocken;
                if (initList != null && initList.parseTocken == ParserTocken.ParserStages.InitialisationList)
                {
                    return ExecuteInitialisationList(node.Children[0].Children);
                }
            }
            return false;
        }

        bool ExecuteInitialisationList(List<TreeNode> declList)
        {
            foreach (TreeNode declaration in declList)
            {
                if (declaration.Children.Count() == 3)
                {
                    int? value = ExecuteDigitExpresion(declaration.Children[2]);
                    if (value != null)
                        CheckIsVariableInitialisation(declaration.Children[0], (int)value);
                    else
                        return false;
                }
            }
            return true;
        }

        bool CheckIsVariableInitialisation(TreeNode node, int value = 0)
        {
            if (intVars.ContainsKey(((IdentifierTocken)node.Tocken).ID))
            {
                intVars[((IdentifierTocken)node.Tocken).ID] = value;
                return true;
            }
            return false;
        }

        int? ExecuteDigitExpresion(TreeNode intExpr)
        {
            if (intExpr.Children.Count == 1)
            {
                return ExecuteSimpleDigitExpresion(intExpr.Children[0]);
            }
            if (intExpr.Children.Count == 3)
            {
                int? leftDgt = ExecuteSimpleDigitExpresion(intExpr.Children[0]), rightDgt = ExecuteSimpleDigitExpresion(intExpr.Children[2]);
                ArithmeticTocken? op = intExpr.Children[1].Children[0].Tocken as ArithmeticTocken;
                if (op != null && leftDgt != null && rightDgt != null)
                {
                    switch (op.operation)
                    {
                        case '+':
                            return leftDgt + rightDgt;
                        case '-':
                            return leftDgt - rightDgt;
                        case '*':
                            return leftDgt * rightDgt;
                        case '/':
                            return leftDgt / rightDgt;
                    }
                }
            }
            return null;
        }

        bool ExecuteFunctionExpresion(TreeNode node)
        {
            ParserTocken? func = node.Children[0].Tocken as ParserTocken;
            if (func != null && func.parseTocken == ParserTocken.ParserStages.Function)
            {
                return ExecuteWrite(node.Children[0]);
            }
            return true;
        }

        int? ExecuteSimpleDigitExpresion(TreeNode node)
        {
            if (node.Children.Count() == 1)
            {
                if (node.Children[0].Tocken is DigitTocken)
                {
                    return ((DigitTocken)node.Children[0].Tocken).digitValue;
                }
                IdentifierTocken? id = node.Children[0].Tocken as IdentifierTocken;
                if (id != null && intVars.TryGetValue(id.ID, out int value))
                {
                    return value;
                }

                ParserTocken? func = node.Children[0].Tocken as ParserTocken;
                if (func != null && func.parseTocken == ParserTocken.ParserStages.Function)
                {
                    return ExecuteRead(node.Children[0]);
                }
            }
            return null;
        }

        StringTocken? ExecuteStringExpression(TreeNode node)
        {
            if (node.Children.Count() == 1)
            {
                StringTocken? str = node.Children[0].Children[0].Tocken as StringTocken;
                return str;
            }
            return null;
        }

        bool ExecuteWrite(TreeNode node)
        {
            if (node.Children.Count() == 2)
            {
                KeyWordTocken? id = node.Children[0].Tocken as KeyWordTocken;
                if (id != null && id.ID == KeyWordTocken.KeyWordID.Write)
                {
                    StringTocken? str = ExecuteStringExpression(node.Children[1].Children[0]);
                    if (str != null)
                    {
                        write.Write(str.stringValue);
                        return true;
                    }

                    int? value = ExecuteDigitExpresion(node.Children[1].Children[0]);
                    if (value != null)
                    {
                        write.Write(((int)value).ToString());
                        return true;
                    }

                }

            }
            return false;
        }

        int? ExecuteRead(TreeNode node)
        {
            if (node.Children.Count() == 1)
            {
                KeyWordTocken? id = node.Children[0].Tocken as KeyWordTocken;
                if (id != null && id.ID == KeyWordTocken.KeyWordID.Read)
                {
                    return read.Read();
                }

            }
            return null;
        }
        private Dictionary<int, int> intVars = new Dictionary<int, int>();
        private InterpreterContext context;
    }
}
