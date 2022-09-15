using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelingSalesman
{
    public class TreeNode
    {

        public float[] Self { get; set; }
        
        public float[] Parent { get; set; }

        public int SelfIndex { get; set; }

        public int ParentIndex { get; set; }

        public List<TreeNode> Children { get; set; }

        public TreeNode()
        {
            Self = null;
            Parent = null;
            SelfIndex = -1;
            ParentIndex = -2;
            Children = new List<TreeNode>();
        }
    }
}
