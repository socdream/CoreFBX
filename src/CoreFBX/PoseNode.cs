using CoreMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreFBX.FBX
{
    public class PoseNode
    {
        /// <summary>
        /// This Id is the same as for a model node
        /// </summary>
        public long Id { get; set; }
        public float[] Matrix { get; set; } = new float[]
        {
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        };

        public float[] Rotation
        {
            get
            {
                return Matrix.QuaternionFromRotationMatrix();
            }
        }

        public float[] Translation
        {
            get
            {
                return Matrix.TranslationFromMatrix();
            }
        }

        public float[] Scaling
        {
            get
            {
                return Matrix.ScalingFromMatrix();
            }
        }

        public bool IsBindPose { get; set; }

        public PoseNode() { }

        public PoseNode(FBXFileNode node, FBXFileNode parent)
        {
            Id = (long)node.Nodes.Where(a => a.Name == "Node").FirstOrDefault()?.Properties[0].Data;
            Matrix = ((double[])node.Nodes.Where(a => a.Name == "Matrix").FirstOrDefault()?.Properties[0].Data).Select(a => (float)a).ToArray().TransposeMatrix();
            IsBindPose = (parent.Properties.Count >= 3 && parent.Properties[2].Data is string) ? ((string)parent.Properties[2].Data) == "BindPose" : false;
        }
    }
}
