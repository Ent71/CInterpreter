using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CInterpreter
{
    public class Parser : IParser
    {
        public Parser(InterpreterContext context)
        {
            this.context = context;
        }
        private IReadOnlyList<Tocken> TockenRow = new List<Tocken>();
        private int row;
        public TreeNode? ParseLine(IReadOnlyList<Tocken> tockenRow, int row)
        {
            this.row = row;
            TockenRow = tockenRow;
            TreeNode root = new TreeNode(new ParserTocken(ParserTocken.ParserStages.Statement, 0, 0));
            if (ParseStmt(root))
            {
                return root;
            }
            return null;
        }

        public bool ParseStmt(TreeNode root)
        {

            int startPos = 0;
            if (TockenRow.Count > 2 && TockenRow[0] is KeyWordTocken && TockenRow[1] is IdentifierTocken)
            {
                return ParseDeclarationStmt(root, ref startPos);
            }
            if (TockenRow.Count > 3 && (TockenRow[0] is IdentifierTocken || TockenRow[0] is KeyWordTocken) && TockenRow[1] is DelimeterTocken && ((DelimeterTocken)TockenRow[1]).delimeter == '(')
            {
                return ParseFunctionStmt(root, ref startPos);
            }
            if (TockenRow.Count > 1 && (TockenRow[0] is IdentifierTocken || TockenRow[0] is KeyWordTocken))
            {
                return ParseInitialisationStmt(root, ref startPos);
            }
            Console.WriteLine("Parser Error: Expected statement on row: {0}, column {1}", TockenRow[startPos].row, TockenRow[startPos].column);
            return false;
        }

        bool ParseDeclarationStmt(TreeNode node, ref int pos)
        {
            if (TockenRow.Count - pos > 0)
            {
                TreeNode currentNode1 = new TreeNode(new ParserTocken(ParserTocken.ParserStages.DeclarationStatement, TockenRow[pos].row, TockenRow[pos].column));
                TreeNode currentNode = new TreeNode(new ParserTocken(ParserTocken.ParserStages.InitialisationList, TockenRow[pos].row, TockenRow[pos].column));
                node.AddChild(currentNode1);
                if (ParseAttribute(currentNode1, ref pos))
                {
                    if (ParseInitialisation(currentNode, ref pos))
                    {
                        if (ParseInitialisationListRest(currentNode, ref pos))
                        {
                            if (ParseDelimeter(';', ref pos))
                            {
                                currentNode1.AddChild(currentNode);
                            }
                        }
                    }

                    return true;
                }

            }


            return false;
        }
        bool ParseInitialisationStmt(TreeNode node, ref int pos)
        {
            if (TockenRow.Count - pos > 0)
            {

                TreeNode currentNode1 = new TreeNode(new ParserTocken(ParserTocken.ParserStages.InitialisationStatement, TockenRow[pos].row, TockenRow[pos].column));
                TreeNode currentNode = new TreeNode(new ParserTocken(ParserTocken.ParserStages.InitialisationList, TockenRow[pos].row, TockenRow[pos].column));
                node.AddChild(currentNode1);
                currentNode1.AddChild(currentNode);
                if (ParseInitialisation(currentNode, ref pos))
                {
                    if (ParseInitialisationListRest(currentNode, ref pos))
                    {
                        if (ParseDelimeter(';', ref pos))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            return false;
        }

        bool ParseFunctionStmt(TreeNode node, ref int pos)
        {
            if (TockenRow.Count - pos > 0)
            {
                TreeNode currentNode = new TreeNode(new ParserTocken(ParserTocken.ParserStages.ExpressionStatement, TockenRow[pos].row, TockenRow[pos].column));
                TreeNode currentNode1 = new TreeNode(new ParserTocken(ParserTocken.ParserStages.Expression, TockenRow[pos].row, TockenRow[pos].column));
                TreeNode currentNode2 = new TreeNode(new ParserTocken(ParserTocken.ParserStages.SimpleExpression, TockenRow[pos].row, TockenRow[pos].column));
                currentNode.AddChild(currentNode1);
                currentNode1.AddChild(currentNode2);
                node.AddChild(currentNode);
                return ParseFunction(currentNode2, ref pos) && ParseDelimeter(';', ref pos);
            }
            return false;
        }

        bool ParseExpression(TreeNode node, ref int pos)
        {
            if (TockenRow.Count - pos > 0)
            {
                TreeNode currentNode = new TreeNode(new ParserTocken(ParserTocken.ParserStages.Expression, TockenRow[pos].row, TockenRow[pos].column));
                node.AddChild(currentNode);
                if (ParseSimpleExpression(currentNode, ref pos))
                {
                    if (TockenRow.Count - pos > 0 && TockenRow[pos] is ArithmeticTocken)
                    {
                        return ParseOperation(currentNode, ref pos) && ParseSimpleExpression(currentNode, ref pos);
                    }
                    return true;
                }
            }
            Console.WriteLine("Parser Error: Expected expresion on row: {0}, column {1}", TockenRow[pos].row, TockenRow[pos].column);
            return false;
        }

        bool ParseAttribute(TreeNode node, ref int pos)
        {
            if (!TryParseAttribute(node, ref pos))
            {
                Console.WriteLine("Parser Error: Expected attribute on row: {0}, column {1}", TockenRow[pos].row, TockenRow[pos].column);
                return false;
            }
            return true;
        }
        bool TryParseAttribute(TreeNode node, ref int pos)
        {
            if (TockenRow.Count - pos > 0)
            {
                KeyWordTocken? tocken = TockenRow[pos] as KeyWordTocken;
                if (tocken != null && tocken.ID == KeyWordTocken.KeyWordID.Int)
                {
                    TreeNode currentNode = new TreeNode(tocken);
                    node.AddChild(currentNode);
                    pos++;
                    return true;
                }
            }
            return false;
        }
        bool ParseInitialisation(TreeNode node, ref int pos)
        {
            if (TockenRow.Count - pos > 0)
            {
                TreeNode currentNode = new TreeNode(new ParserTocken(ParserTocken.ParserStages.Initialisation, TockenRow[pos].row, TockenRow[pos].column));
                node.AddChild(currentNode);
                if (ParseIdOrKeyWord(currentNode, ref pos))
                {
                    if (TockenRow[pos] is AssigmentTocken)
                    {
                        return ParseAssigment(currentNode, ref pos) && ParseExpression(currentNode, ref pos);
                    }
                    return true;
                }

            }
            Console.WriteLine("Parser Error: Expected initialiation on row: {0}, column {1}", TockenRow[pos].row, TockenRow[pos].column);
            return false;
        }
        bool ParseInitialisationListRest(TreeNode node, ref int pos)
        {
            int curPos = pos;
            bool isWork = true;
            do
            {
                if (TockenRow.Count - curPos > 0 && TockenRow[pos] is DelimeterTocken && ((DelimeterTocken)TockenRow[pos]).delimeter == ',')
                {
                    isWork = ParseDelimeter(',', ref curPos) && ParseInitialisation(node, ref curPos) && ParseInitialisationListRest(node, ref curPos);
                    if (isWork)
                    {
                        pos = curPos;
                    }
                    else
                    {
                        Console.WriteLine("Parser Error: Expected initialiations on row: {0}, column {1}", TockenRow[pos].row, TockenRow[pos].column);
                        return false;
                    }
                }
                else
                {
                    isWork = false;
                }
            } while (isWork);

            return true;
        }
        bool ParseSimpleExpression(TreeNode node, ref int pos)
        {
            if (TockenRow.Count - pos > 0)
            {
                TreeNode currentNode = new TreeNode(new ParserTocken(ParserTocken.ParserStages.SimpleExpression, TockenRow[pos].row, TockenRow[pos].column));
                node.AddChild(currentNode);
                switch (TockenRow[pos])
                {
                    case KeyWordTocken _:
                    case IdentifierTocken _:
                        if (TockenRow.Count - pos > 1 && TockenRow[pos + 1] is DelimeterTocken && ((DelimeterTocken)TockenRow[pos + 1]).delimeter == '(')
                        {
                            return ParseFunction(currentNode, ref pos);
                        }
                        return ParseIdOrKeyWord(currentNode, ref pos);
                    case DigitTocken:
                        return ParseDigit(currentNode, ref pos);
                    case StringTocken:
                        return ParseString(currentNode, ref pos);
                }
            }

            Console.WriteLine("Parser Error: Expected simple expression on row: {0}, column {1}", TockenRow[pos].row, TockenRow[pos].column);
            return false;
        }

        bool ParseOperation(TreeNode node, ref int pos)
        {
            if (TockenRow.Count - pos > 0)
            {
                TreeNode currentNode = new TreeNode(new ParserTocken(ParserTocken.ParserStages.Operation, TockenRow[pos].row, TockenRow[pos].column));
                node.AddChild(currentNode);
                if (TockenRow[pos] is ArithmeticTocken)
                {
                    currentNode.AddChild(new TreeNode((ArithmeticTocken)TockenRow[pos]));
                    pos++;
                    return true;
                }
            }

            Console.WriteLine("Parser Error: Expected operation on row: {0}, column {1}", TockenRow[pos].row, TockenRow[pos].column);
            return false;
        }
        bool ParseFunction(TreeNode node, ref int pos)
        {
            if (TockenRow.Count - pos > 0)
            {
                TreeNode currentNode = new TreeNode(new ParserTocken(ParserTocken.ParserStages.Function, TockenRow[pos].row, TockenRow[pos].column));
                node.AddChild(currentNode);
                return ParseIdOrKeyWord(currentNode, ref pos) && ParseDelimeter('(', ref pos) && ParseFunctionArguments(currentNode, ref pos) && ParseDelimeter(')', ref pos);
            }

            Console.WriteLine("Parser Error: Expected function call on row: {0}, column {1}", TockenRow[pos].row, TockenRow[pos].column);

            return false;
        }
        bool ParseFunctionArguments(TreeNode node, ref int pos)
        {
            if (TockenRow.Count - pos > 0 && (!(TockenRow[pos] is DelimeterTocken) || ((DelimeterTocken)TockenRow[pos]).delimeter != ')'))
            {
                TreeNode currentNode = new TreeNode(new ParserTocken(ParserTocken.ParserStages.Arguments, TockenRow[pos].row, TockenRow[pos].column));
                if (ParseExpression(currentNode, ref pos) && ParseFunctionArgumentsListRest(currentNode, ref pos))
                {
                    node.AddChild(currentNode);
                }
            }

            return true;
        }
        bool ParseFunctionArgumentsListRest(TreeNode node, ref int pos)
        {
            int curPos = pos;
            bool isWork = true;
            do
            {
                if (TockenRow.Count - curPos > 0 && TockenRow[pos] is DelimeterTocken && ((DelimeterTocken)TockenRow[pos]).delimeter == ',')
                {
                    isWork = ParseDelimeter(',', ref curPos) && ParseExpression(node, ref curPos) && ParseFunctionArgumentsListRest(node, ref curPos);
                    if (isWork)
                    {
                        pos = curPos;
                    }
                    else
                    {
                        Console.WriteLine("Parser Error: Expected args on row: {0}, column {1}", TockenRow[pos].row, TockenRow[pos].column);

                        return false;
                    }
                }
                else
                {
                    isWork = false;
                }

            } while (isWork);

            return true;
        }

        bool ParseDelimeter(char delimeter, ref int pos)
        {

            if (TockenRow.Count - pos > 0)
            {
                DelimeterTocken? tocken = TockenRow[pos] as DelimeterTocken;
                if (tocken != null && tocken.delimeter == delimeter)
                {
                    pos++;
                    return true;
                }
                else
                { 
                    Console.WriteLine("Parser Error: Expected \'{0}\' on row: {1}, column {2}", delimeter, TockenRow[pos].row, TockenRow[pos].column);
                }
            }
            return false;
        }

        bool ParseIdOrKeyWord(TreeNode node, ref int pos)
        {
            if (TryParseIdentifier(node, ref pos) || TryParseKeyWord(node, ref pos))
            {
                return true;
            }
            Console.WriteLine("Parser Error: id or kw on row: {0}, column {1}", TockenRow[pos].row, TockenRow[pos].column);

            return false;
        }

        bool ParseIdentifier(TreeNode node, ref int pos)
        {
            if (TryParseIdentifier(node, ref pos))
            {
                return true;
            }
            Console.WriteLine("Parser Error: id on row: {0}, column {1}", TockenRow[pos].row, TockenRow[pos].column);
            return false;
        }
        bool TryParseIdentifier(TreeNode node, ref int pos)
        {

            if (TockenRow.Count - pos > 0)
            {
                IdentifierTocken? tocken = TockenRow[pos] as IdentifierTocken;
                if (tocken != null)
                {
                    node.AddChild(new TreeNode(tocken));
                    pos++;
                    return true;
                }
            }
            return false;
        }

        bool ParseKeyWord(TreeNode node, ref int pos)
        {
            if (TryParseKeyWord(node, ref pos))
            {
                return true;
            }
            Console.WriteLine("Parser Error: keyw on row: {0}, column {1}", TockenRow[pos].row, TockenRow[pos].column);
            return false;
        }

        bool TryParseKeyWord(TreeNode node, ref int pos)
        {

            if (TockenRow.Count - pos > 0)
            {
                KeyWordTocken? tocken = TockenRow[pos] as KeyWordTocken;
                if (tocken != null)
                {
                    node.AddChild(new TreeNode(tocken));
                    pos++;
                    return true;
                }
            }
            return false;
        }

        bool ParseAssigment(TreeNode node, ref int pos)
        {

            if (TockenRow.Count - pos > 0)
            {
                AssigmentTocken? tocken = TockenRow[pos] as AssigmentTocken;
                if (tocken != null)
                {
                    node.AddChild(new TreeNode(tocken));
                    pos++;
                    return true;
                }
            }
            Console.WriteLine("Parser Error: Expected \'=\' on row: {0}, column {1}", TockenRow[pos].row, TockenRow[pos].column);

            return false;
        }

        bool ParseDigit(TreeNode node, ref int pos)
        {

            if (TockenRow.Count - pos > 0)
            {
                DigitTocken? tocken = TockenRow[pos] as DigitTocken;
                if (tocken != null)
                {
                    node.AddChild(new TreeNode(tocken));
                    pos++;
                    return true;
                }
            }

            Console.WriteLine("Parser Error: Expected digit on row: {0}, column {1}", TockenRow[pos].row, TockenRow[pos].column);

            return false;
        }

        bool ParseString(TreeNode node, ref int pos)
        {

            if (TockenRow.Count - pos > 0)
            {
                StringTocken? tocken = TockenRow[pos] as StringTocken;
                if (tocken != null)
                {
                    node.AddChild(new TreeNode(tocken));
                    pos++;
                    return true;
                }
            }
            Console.WriteLine("Parser Error: Expected string on row: {0}, column {1}", TockenRow[pos].row, TockenRow[pos].column);


            return false;
        }
        public void DumpParserTree(TreeNode root, TextWriter writer)
        {
            DumpParserTreeHelper(root, writer, 0);
        }
        private void DumpParserTreeHelper(TreeNode node, TextWriter writer, int depth)
        {
            writer.Write(new string('.', depth * 2));
            context.DumpTocken(node.Tocken, writer);
            writer.WriteLine();
            foreach (var child in node.Children)
            {
                DumpParserTreeHelper(child, writer, depth + 1);
            }
        }

        private InterpreterContext context;
    }
}
