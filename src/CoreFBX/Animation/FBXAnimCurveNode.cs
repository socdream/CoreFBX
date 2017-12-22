using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreFBX.FBX.Animation
{
    /// <summary>
    /// <para>An animation curve node (FbxAnimCurveNode) is the connection point between 
    /// the animation curves and the FBX properties. To connect an animation curve 
    /// to a FBX property, you can connect the animation curve and the FBX property 
    /// to one animation curve node.</para>
    /// <para>An animation curve node can be connected to only one FBX property of 
    /// one FBX object. FBX properties such as FbxNode::LclTranslation contain more 
    /// than one value (X, Y, Z). If you create an animation curve node by calling 
    /// FbxAnimCurveNode::CreateTypedCurveNode, you must specify a FBX property of 
    /// the data type you need, for example, the function CreateTypedCurveNode() in 
    /// LclTranslation creates an animation curve node with the necessary animation 
    /// channels. In this example, channels X, Y, and Z. However, the animation curve node 
    /// is not actually connected to the specified FBX property. </para>
    /// <para>To allow the same FBX property to have different values on each 
    /// animation layer, an animation curve node must be connected to a single 
    /// animation layer. If the same animation curve node is connected to multiple 
    /// animation layers, the same value is used for the FBX property in each of 
    /// these animation layers.</para>
    /// </summary>
    public class FBXAnimCurveNode
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public float[] Value { get { return new float[] { ValueX, ValueY, ValueZ }; } }

        public string Attr { get; set; }
        public bool AttrX { get; set; } = false;
        public bool AttrY { get; set; } = false;
        public bool AttrZ { get; set; } = false;

        public float ValueX { get; set; }
        public float ValueY { get; set; }
        public float ValueZ { get; set; }

        /// <summary>
        /// Parents ids
        /// </summary>
        public long[] ContainerIndices { get; set; }
        /// <summary>
        /// Children ids
        /// </summary>
        public long[] CurveIdx { get; set; }
        public long ContainerBoneId { get; set; }
        public long ContainerId { get; set; }

        public Dictionary<string, FBXAnimCurve> Curves { get; set; } = new Dictionary<string, FBXAnimCurve>();

        public FBXAnimCurveNode(FBXFileNode node)
        {
            Id = node.Id;
            Name = node.NodeName;
            Attr = Name;

            var propsNode = node.Nodes.Where(a => a.Name == "Properties70").FirstOrDefault();
            
            if (Name.Contains("T") || Name.Contains("R") || Name.Contains("S"))
            {
                foreach (var propNode in propsNode.Nodes)
                {
                    foreach (var prop in propNode.Properties)
                        if (prop.Data is string && (prop.Data as string).Contains("X"))
                        {
                            AttrX = true;
                            ValueX = (float)(double)propNode.Properties[4].Data;
                        }
                        else if (prop.Data is string && (prop.Data as string).Contains("Y"))
                        {
                            AttrY = true;
                            ValueY = (float)(double)propNode.Properties[4].Data;
                        }
                        else if (prop.Data is string && (prop.Data as string).Contains("Z"))
                        {
                            AttrZ = true;
                            ValueZ = (float)(double)propNode.Properties[4].Data;
                        }
                }
            }
        }

        public FBXAnimCurveNode(FBXFileNode node, FBXFile file, List<string> bones)
        {
            Id = node.Id;
            Name = node.NodeName;
            Attr = Name;

            var propsNode = node.Nodes.Where(a => a.Name == "Properties70").FirstOrDefault();
            
            if (Name.Contains("T") || Name.Contains("R") || Name.Contains("S"))
            {
                foreach (var propNode in propsNode.Nodes)
                {
                    foreach (var prop in propNode.Properties)
                        if (prop.Data is string && (prop.Data as string).Contains("X"))
                        {
                            AttrX = true;
                            ValueX = (float)(double)propNode.Properties[4].Data;
                        }
                        else if (prop.Data is string && (prop.Data as string).Contains("Y"))
                        {
                            AttrY = true;
                            ValueY = (float)(double)propNode.Properties[4].Data;
                        }
                        else if (prop.Data is string && (prop.Data as string).Contains("Z"))
                        {
                            AttrZ = true;
                            ValueZ = (float)(double)propNode.Properties[4].Data;
                        }
                }
            }

            ContainerIndices = file.Connections.Where(a => a.Src == node.Id).Select(a => a.Dst).ToArray();
            CurveIdx = file.Connections.Where(a => a.Dst == node.Id).Select(a => a.Src).ToArray();

            for (int i = ContainerIndices.Length - 1; i >= 0; i--)
            {
                // gets the real id of the bone
                /*var boneId = bones.TakeWhile(a => a.Id != ContainerIndices[i]).Count();

                if(boneId >= 0)
                {
                    ContainerBoneId = boneId;
                    ContainerId = ContainerIndices[i];

                    break;
                }*/
            }
        }
    }
}
