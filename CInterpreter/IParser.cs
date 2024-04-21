using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CInterpreter
{
    public interface IParser
    {
        public TreeNode? ParseLine(IReadOnlyList<Tocken> tockenRow, int row);
        public void DumpParserTree(TreeNode root, TextWriter writer);
    }
}
