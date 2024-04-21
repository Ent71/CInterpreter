using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CInterpreter
{
    public class TreeNode
    {
        public Tocken Tocken { get; set; }

        public List<TreeNode> Children { get; set; }

        public bool AddChild(TreeNode child)
        {
            Children.Add(child);
            return true;
        }

        public TreeNode(Tocken tocken)
        {
            Tocken = tocken;
            Children = new List<TreeNode>();
        }
    }
}
