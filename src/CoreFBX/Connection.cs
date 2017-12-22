using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreFBX.FBX
{
    public class Connection
    {
        /// <summary>
        /// A code string defining the nature of the relation (so far, found "OO" for object linked to object, 
        /// and "OP" for object linked to property.
        /// </summary>
        public string Relation { get; set; }
        /// <summary>
        /// Child Id of the connection
        /// </summary>
        public long Src { get; set; } = 0;
        /// <summary>
        /// Parent Id of the connection
        /// </summary>
        public long Dst { get; set; } = 0;
        public string ConnectionType { get; set; }

        public Connection() { }

        public Connection(FBXFileNode node)
        {
            Relation = (string)node.Properties[0].Data;
            Src = (long)node.Properties[1].Data;
            Dst = (long)node.Properties[2].Data;

            if (node.Properties.Count > 3)
                ConnectionType = (string)node.Properties[3].Data;
        }
    }
}
