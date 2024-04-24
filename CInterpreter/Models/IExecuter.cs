using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CInterpreter.Models
{
    public interface IExecuter
    {
        public bool ExecuteProgram(TreeNode node);
        public void dumpError(TextWriter writer);
        public void Reset();
    }
}