using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CInterpreter.Models;

namespace CInterpreter
{
    public class Parser : IParser
    {
        private InterpreterContext context;
        private List<string> errorMessages = new List<string>();
        private IReadOnlyList<Tocken> TockenRow = new List<Tocken>();

        public Parser(InterpreterContext context)
        {
            this.context = context;
        }

        public void Reset()
        {
            errorMessages.Clear();
        }

        public TreeNode? ParseLine(IReadOnlyList<Tocken> tockenRow, int row)
        {
            errorMessages.Clear();
            TockenRow = tockenRow;
            if(tockenRow.Count() > 0)
            {
                TreeNode root = new TreeNode(new ParserTocken(ParserTocken.ParserStages.Statement, TockenRow[0].row, TockenRow[0].column));
                if (ParseStmt(root))
                {
                    return root;
                }
            }
            return null;
        }

        private bool ParseStmt(TreeNode root)
        {

            int startPos = 0;
            if (TockenRow.Count > 2 && TockenRow[0] is KeyWordTocken && TockenRow[1] is IdentifierTocken)
            {
                return ParseDeclarationStmt(root, ref startPos);
            }
            if (TockenRow.Count > 2 && (TockenRow[0] is IdentifierTocken || TockenRow[0] is KeyWordTocken) && TockenRow[1] is DelimeterTocken && ((DelimeterTocken)TockenRow[1]).delimeter == '(')
            {
                return ParseFunctionStmt(root, ref startPos);
            }
            if (TockenRow.Count > 1 && (TockenRow[0] is IdentifierTocken || TockenRow[0] is KeyWordTocken))
            {
                return ParseInitialisationStmt(root, ref startPos);
            }
            SaveParserError("Declaration, Initialization or Function Call", startPos);
            return false;
        }

        private bool ParseDeclarationStmt(TreeNode node, ref int pos)
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
                                return true;
                            }
                        }
                    }

                }

            }

            return false;
        }

        private bool ParseInitialisationStmt(TreeNode node, ref int pos)
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
            }

            return false;
        }

        private bool ParseFunctionStmt(TreeNode node, ref int pos)
        {
            if (TockenRow.Count - pos > 0)
            {
                TreeNode currentNode = new TreeNode(new ParserTocken(ParserTocken.ParserStages.ExpressionStatement, TockenRow[pos].row, TockenRow[pos].column));
                node.AddChild(currentNode);
                return ParseFunction(currentNode, ref pos) && ParseDelimeter(';', ref pos);
            }

            return false;
        }

        private bool ParseExpression(TreeNode node, ref int pos)
        {
            if (TockenRow.Count - pos > 0 && (TockenRow[pos] is DigitTocken || TockenRow[pos] is KeyWordTocken || TockenRow[pos] is IdentifierTocken || TockenRow[pos] is StringTocken))
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

            SaveParserError("Expression", pos);
            return false;
        }

        private bool ParseAttribute(TreeNode node, ref int pos)
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
            SaveParserError("Type", pos);
            return false;
        }
        private bool ParseInitialisation(TreeNode node, ref int pos)
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
            SaveParserError("Initialization", pos);
            return false;
        }
        private bool ParseInitialisationListRest(TreeNode node, ref int pos)
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
        private bool ParseSimpleExpression(TreeNode node, ref int pos)
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

            SaveParserError("Simple Expression", pos);
            return false;
        }

        private bool ParseOperation(TreeNode node, ref int pos)
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

            SaveParserError("Operation", pos);
            return false;
        }

        private bool ParseFunction(TreeNode node, ref int pos)
        {
            if (TockenRow.Count - pos > 0)
            {
                TreeNode currentNode = new TreeNode(new ParserTocken(ParserTocken.ParserStages.Function, TockenRow[pos].row, TockenRow[pos].column));
                node.AddChild(currentNode);
                return ParseIdOrKeyWord(currentNode, ref pos) && ParseDelimeter('(', ref pos) && ParseFunctionArguments(currentNode, ref pos) && ParseDelimeter(')', ref pos);
            }

            SaveParserError("Function Call", pos);
            return false;
        }

        private bool ParseFunctionArguments(TreeNode node, ref int pos)
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

        private bool ParseFunctionArgumentsListRest(TreeNode node, ref int pos)
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
                        SaveParserError("Function Argument", pos);
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

        private bool ParseDelimeter(char delimeter, ref int pos)
        {

            if (TockenRow.Count - pos > 0)
            {
                DelimeterTocken? tocken = TockenRow[pos] as DelimeterTocken;
                if (tocken != null && tocken.delimeter == delimeter)
                {
                    pos++;
                    return true;
                }
            }
            SaveParserError(string.Format("\'{0}\'", delimeter), pos);
            return false;
        }

        private bool ParseIdOrKeyWord(TreeNode node, ref int pos)
        {
            if (TryParseIdentifier(node, ref pos) || TryParseKeyWord(node, ref pos))
            {
                return true;
            }

            SaveParserError("Identifier or Keyword", pos);
            return false;
        }

        private bool TryParseIdentifier(TreeNode node, ref int pos)
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

        private bool TryParseKeyWord(TreeNode node, ref int pos)
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

        private bool ParseAssigment(TreeNode node, ref int pos)
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

            SaveParserError("\'=\'", pos);
            return false;
        }

        private bool ParseDigit(TreeNode node, ref int pos)
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

            SaveParserError("Number", pos);
            return false;
        }

        private bool ParseString(TreeNode node, ref int pos)
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

            SaveParserError("String", pos);
            return false;
        }

        private void SaveParserError(string message, int pos)
        {
            int row = 1, column = 1;
            
            if(TockenRow.Count() > 1)
            {
                if(TockenRow.Count() - pos > 0 && TockenRow[pos].row == TockenRow[pos - 1].row)
                {
                    row = TockenRow[pos].row;
                    column = TockenRow[pos].column;
                }
                else
                {
                    row = TockenRow[pos - 1].row;
                    column = context.endOfRowPositions[row - 1];
                }

            }

            errorMessages.Add(string.Format("Parser Error: Expected {0} on row: {1}, column {2}", message, row, column));
        }

        public void dumpError(TextWriter writer)
        {
            foreach(string message in errorMessages)
            {
                writer.WriteLine(message);
            }
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
    }
}