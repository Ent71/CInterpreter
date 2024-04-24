using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CInterpreter.Models;

namespace CInterpreter
{
    public class Executer : IExecuter
    {
        private Dictionary<int, int> intVars = new Dictionary<int, int>();
        private InterpreterContext context;
        private IReadable read;
        private IWritable write;
        private string errorMessage = "";

        public Executer(InterpreterContext context, IReadable read, IWritable write)
        {
            this.context = context;
            this.read = read;
            this.write = write;
        }

        public void Reset()
        {
            errorMessage = "";
            intVars.Clear();
            context.Reset();
        }

        public bool ExecuteProgram(TreeNode node)
        {
            errorMessage = "";
            if(!ExecuteStatement(node))
            {
                if (errorMessage == "")
                {
                    SaveExecuterError("Parser and Executor mismatch", node.Tocken.row, node.Tocken.column);
                }
                return false;
            }
            return true;
        }

        private bool ExecuteStatement(TreeNode node)
        {
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
            if(node.Children.Count() == 1)
            {
                return ExecuteFunctionExpresion(node.Children[0]);
            }
            return false;
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
                    if(!ChecIsVariableDeclare(declaration.Children[0]))
                    {
                        return false;
                    }
                }
                if (declaration.Children.Count() == 3)
                {
                    int? value = ExecuteDigitExpresion(declaration.Children[2]);
                    if (value != null)
                    {
                        if(!ChecIsVariableDeclare(declaration.Children[0], (int)value))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
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
            SaveExecuterError("Variable already declared", node.Tocken.row, node.Tocken.column);
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
                    {

                        if(!CheckIsVariableInitialisation(declaration.Children[0], (int)value))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
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
            SaveExecuterError("Undeclared variable", node.Tocken.row, node.Tocken.column);
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
                            if (rightDgt == 0)
                            {
                                SaveExecuterError("Division by zero", intExpr.Tocken.row, intExpr.Tocken.column);
                                return null;
                            }
                            return leftDgt / rightDgt;
                    }
                }
            }
            return null;
        }

        bool ExecuteFunctionExpresion(TreeNode node)
        {
            ParserTocken? func = node.Tocken as ParserTocken;
            if (func != null && func.parseTocken == ParserTocken.ParserStages.Function)
            {
                return ExecuteFunction(node);
            }
            return false;
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
                if (id != null)
                {
                    if(intVars.TryGetValue(id.ID, out int value))
                    {
                        return value;
                    }
                    else
                    {
                        SaveExecuterError("Undeclared variable", node.Tocken.row, node.Tocken.column);
                        return null;
                    }
                }

                ParserTocken? func = node.Children[0].Tocken as ParserTocken;
                if (func != null && func.parseTocken == ParserTocken.ParserStages.Function)
                {
                    return ExecuteIntFunction(node.Children[0]);
                }
            }
            return null;
        }

        bool ExecuteFunction(TreeNode node)
        {
            if(IsStandartFunc(node, KeyWordTocken.KeyWordID.Write))
            {
                return ExecuteWrite(node);
            }
            if (IsStandartFunc(node, KeyWordTocken.KeyWordID.Read))
            {
                return ExecuteRead(node) == null ? false : true;
            }

            SaveExecuterError("Undefined function", node.Tocken.row, node.Tocken.column); 
            return false;
        }

        int? ExecuteIntFunction(TreeNode node)
        {
            if (IsStandartFunc(node, KeyWordTocken.KeyWordID.Read))
            {
                return ExecuteRead(node);
            }

            SaveExecuterError("Expected \'Read\'", node.Tocken.row, node.Tocken.column);
            return null;
        }
        bool IsStandartFunc(TreeNode node, KeyWordTocken.KeyWordID funcId)
        {
            if (node.Children.Count() > 0)
            {
                KeyWordTocken? funcName = node.Children[0].Tocken as KeyWordTocken;
                if (funcName != null && funcName.ID == funcId)
                {
                    return true;
                }
            }
            return false;
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
            SaveExecuterError("Invalid args passed to function (Expected 1 int or string argument)", node.Tocken.row, node.Tocken.column);
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
            SaveExecuterError("Invalid args passed to function (Expected 0 arguments)", node.Tocken.row, node.Tocken.column);
            return null;
        }

        private void SaveExecuterError(string message, int row, int column)
        {
            errorMessage = string.Format("Executer Error: {0} line: {1}, column: {2}", message, row, column);
        }

        public void dumpError(TextWriter output)
        {
            if (errorMessage.Length != 0)
            {
                output.WriteLine(errorMessage);
            }
        }
    }
}
