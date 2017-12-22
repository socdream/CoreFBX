using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreFBX.FBX.Animation
{
    /// <summary>
    /// An animation layer contains one or more animation curve nodes that are connected 
    /// to the animation curves. The animation stack must have at least one animation 
    /// layer known as the base layer. For blended animation, more than one animation 
    /// layer is required.
    /// </summary>
    public class FBXAnimLayer
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public List<FBXAnimCurveNode> AnimationCurveNodes { get; set; } = new List<FBXAnimCurveNode>();

        public FBXAnimLayer(FBXFileNode node)
        {
            Id = node.Id;
            Name = node.Name;
        }
    }
}
