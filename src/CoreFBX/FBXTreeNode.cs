using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreFBX.FBX
{
    public class FBXTreeNode
    {
        public List<FBXTreeNode> Children { get; set; } = new List<FBXTreeNode>();
        public FBXFileNode Node { get; set; }
        public FBXTreeNode Parent { get; set; }

        public FBXTreeNode() { }

        public FBXTreeNode(FBXFileNode node, FBXFile file, FBXTreeNode parent = null)
        {
            Node = node;

            var nodeId = (Node != null) ? Node.Id : 0;

            var nodeConns = file.Connections.Where(a => a.Dst == nodeId);
            
            foreach(var con in nodeConns)
            {
                var childNode = file.FindChild(con.Src);

                if (childNode != null)
                    Children.Add((new FBXTreeNode(childNode, file, this)));
            }
        }

        public string GetStructure(int padding)
        {
            var pad = new string(' ', padding);

            var result = "";

            if (Node != null)
                result = pad + Node.Id + (!string.IsNullOrEmpty(Node.NodeName) ? " (" + Node.NodeName + ")" : "") + " - " + Node.Name;
            else
                result = pad + "Root";
            
            foreach (var child in Children)
                result += "\r\n" + child.GetStructure(padding + 2);

            return result;
        }

        public FBXTreeNode FindChild(long id)
        {
            if (Node != null && Node.Id == id)
                return this;

            foreach (var node in Children)
            {
                var child = node.FindChild(id);

                if (child != null)
                    return child;
            }

            return null;
        }

        public FBXTreeNode FindChild(string name)
        {
            if (Node != null && Node.NodeName == name)
                return this;

            foreach (var node in Children)
            {
                var child = node.FindChild(name);

                if (child != null)
                    return child;
            }

            return null;
        }
    }
}
