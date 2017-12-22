using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreFBX.FBX.Animation
{
    /// <summary>
    /// The animation stack is the highest-level container for the animation data and 
    /// contains one or more animation layers.
    /// </summary>
    public class FBXAnimStack
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public List<FBXAnimLayer> AnimationLayers { get; set; } = new List<FBXAnimLayer>();

        public FBXAnimStack(FBXFileNode node)
        {
            Id = node.Id;
            Name = node.Name;
        }
    }
}
