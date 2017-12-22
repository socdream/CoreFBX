using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreFBX.FBX
{
    /// <summary>
    /// Given a mesh binding with several bones(links), Transform 
    /// is the global transform of the mesh at the binding moment, 
    /// TransformLink is the global transform of the bone(link) at 
    /// the binding moment, TransformAssociateModel is the global 
    /// transform of the associate model at the binding moment.
    /// </summary>
    public class Deformer
    {
        public long Id { get; set; } = 0;
        public string Name { get; set; } = "";
        public FBXDeformerType DeformerType { get; set; } = FBXDeformerType.None;

        public string BoneId { get; set; } = "";

        public int[] Indexes { get; set; } = new int[0];
        public float[] Weights { get; set; } = new float[0];
        /// <summary>
        /// <para>Transform refers to the global initial transform of the geometry node that contains the link node.</para>
        /// <para>Transform is not the "original" Transform.</para>
        /// <para>This is actually: TransformLink.Inverse() * Transform</para>
        /// </summary>
        public float[] Transform { get; set; } = new float[]
        {
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        };

        /// <summary>
        /// TransformLink refers to global initial transform of the link node
        /// </summary>
        public float[] TransformLink { get; set; } = new float[]
        {
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        };

        public float[] BindPoseMatrix { get; set; } = new float[]
        {
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        };

        public Deformer() { }

        public Deformer(FBXFileNode node)
        {
            Id = (long)node.Properties[0].Data;
            Name = ((string)node.Properties[1].Data);

            switch ((string)node.Properties[2].Data)
            {
                case "Skin":
                    DeformerType = FBXDeformerType.Skin;
                    break;
                case "Cluster":
                    DeformerType = FBXDeformerType.Cluster;
                    break;
            }

            if(DeformerType == FBXDeformerType.Cluster)
            {
                var indexesNode = node.Nodes.Where(a => a.Name == "Indexes").FirstOrDefault();

                if (indexesNode != null)
                    Indexes = (int[])indexesNode.Properties[0].Data;

                var weightsNode = node.Nodes.Where(a => a.Name == "Weights").FirstOrDefault();

                if (weightsNode != null)
                    Weights = ((double[])weightsNode.Properties[0].Data).Select(a => (float)a).ToArray();

                var transformNode = node.Nodes.Where(a => a.Name == "Transform").FirstOrDefault();

                if (transformNode != null)
                    Transform = ((double[])transformNode.Properties[0].Data).Select(a => (float)a).ToArray();

                var transformLinkNode = node.Nodes.Where(a => a.Name == "TransformLink").FirstOrDefault();

                if (transformLinkNode != null)
                    TransformLink = ((double[])transformLinkNode.Properties[0].Data).Select(a => (float)a).ToArray();
            }
        }

        public static Deformer FromTreeNode(FBXTreeNode node, List<PoseNode> poseNodes)
        {
            var deformer = new Deformer(node.Node);

            var model = node.Children.Where(a => a.Node.Name == "Model").FirstOrDefault();

            deformer.BindPoseMatrix = GetBindMatrix(model, poseNodes);

            deformer.BoneId = model.Node.Id.ToString();

            return deformer;
        }

        private static float[] GetBindMatrix(FBXTreeNode node, List<PoseNode> poseNodes)
        {
            var result = new float[]
            {
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            };

            var pose = poseNodes.Where(a => a.Id == node.Node.Id && a.IsBindPose).FirstOrDefault();

            // The bind pose is always a global matrix
            if (pose != null)
                result = pose.Matrix;

            /*var childNodes = node.Children.Where(a => a.Node.Name == "Model");

            foreach (var child in childNodes)
                result = GetBindMatrix(child, poseNodes).MatrixProduct(result);*/

            return result;
        }

        public enum FBXDeformerType
        {
            None,
            //Skeleton: there's usually just 1 of these
            Skin,
            //joint container
            Cluster
        }
    }
}
