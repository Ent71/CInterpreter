using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CInterpreter
{
    internal class ParserTocken : Tocken
    {
        public enum ParserStages
        {
            Statement,
            DeclarationStatement,
            InitialisationStatement,
            ExpressionStatement,
            IntAttribute,
            Initialisation,
            InitialisationList,
            ExpressionRest,
            Expression,
            SimpleExpression,
            FunctionCall,
            Operation,
            FirstPriorityOperation,
            SecondPriorityOperation,
            Arguments,
            ArgumentsListRest,
            Digit,
            Identifier,
            Function
        }

        public ParserTocken(ParserStages val, int row, int column)
            : base(row, column)
        {
            parseTocken = val;
        }

        public readonly ParserStages parseTocken;
    }
}