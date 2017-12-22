using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreFBX.FBX.Animation
{
    /// <summary>
    /// <para>An animation curve, also known as function curve or FCurve defines how a 
    /// FBX property (FbxProperty) of a FBX object is animated or different from the 
    /// default value in the animation layer. The animation curves are connected 
    /// to the animation curve nodes. The same animation curve can be connected 
    /// to multiple animation curve nodes regardless of the referred FBX property. 
    /// Accordingly, one animation curve can animate many FBX properties of many 
    /// FBX objects. The values in the animation curves are dependent on the FBX 
    /// property type and is validated by the application.</para>
    /// <para>Animation curves are not mandatory.To save the memory and processing 
    /// time, FBX properties that are not animated can be left without using the 
    /// animation curves.</para>
    /// </summary>
    public class FBXAnimCurve
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public double Default { get; set; }
        public int KeyVer { get; set; }
        /// <summary>
        /// Times
        /// </summary>
        public long[] KeyTime { get; set; }
        public float[] Times { get { return KeyTime.FBXTimeToSeconds(); } }
        /// <summary>
        /// Values
        /// </summary>
        public float[] KeyValueFloat { get; set; }
        public int[] KeyAttrFlags { get; set; }
        public float[] KeyAttrDataFloat { get; set; }
        public int[] KeyAttrRefCount { get; set; }

        public int Length { get { return KeyTime.Length - 1; } }
        
        public FBXAnimCurveNode AnimationCurveNode { get; set; }

        public FBXAnimCurve() { }

        public FBXAnimCurve(FBXFileNode node)
        {
            Id = node.Id;
            Name = node.Name;

            Default = (double)node.Nodes.Where(a => a.Name == "Default").FirstOrDefault()
                .Properties[0].Data;
            KeyVer = (int)node.Nodes.Where(a => a.Name == "KeyVer").FirstOrDefault()
                .Properties[0].Data;
            KeyTime = (long[])node.Nodes.Where(a => a.Name == "KeyTime").FirstOrDefault()
                .Properties[0].Data;
            KeyValueFloat = (float[])node.Nodes.Where(a => a.Name == "KeyValueFloat").FirstOrDefault()
                .Properties[0].Data;
            KeyAttrFlags = (int[])node.Nodes.Where(a => a.Name == "KeyAttrFlags").FirstOrDefault()
                .Properties[0].Data;
            KeyAttrDataFloat = (float[])node.Nodes.Where(a => a.Name == "KeyAttrDataFloat").FirstOrDefault()
                .Properties[0].Data;
            KeyAttrRefCount = (int[])node.Nodes.Where(a => a.Name == "KeyAttrRefCount").FirstOrDefault()
                .Properties[0].Data;
        }
    }
}
