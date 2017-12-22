using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoreFBX.FBX
{
    public class FBXFileNode
    {
        public uint EndOffset { get; set; }
        public uint NumProperties { get; set; }
        public uint PropertyListLen { get; set; }
        public byte NameLen { get; set; }
        public string Name { get; set; }
        public bool IsNull { get; set; } = false;

        public List<Property> Properties { get; set; } = new List<Property>();
        public List<FBXFileNode> Nodes { get; set; } = new List<FBXFileNode>();

        public long Id
        {
            get
            {
                if (Properties.Count > 0 && Properties[0].Data is long)
                    return (long)Properties[0].Data;

                return -1;
            }
        }

        public string NodeName
        {
            get
            {
                if (Properties.Count > 1 && Properties[1].Data is string)
                    return (string)Properties[1].Data;

                return null;
            }
        }

        public FBXFileNode(Stream stream)
        {
            var uintBuffer = new byte[4];
            stream.Read(uintBuffer, 0, 4);
            EndOffset = BitConverter.ToUInt32(uintBuffer, 0);

            stream.Read(uintBuffer, 0, 4);
            NumProperties = BitConverter.ToUInt32(uintBuffer, 0);

            stream.Read(uintBuffer, 0, 4);
            PropertyListLen = BitConverter.ToUInt32(uintBuffer, 0);

            NameLen = (byte)stream.ReadByte();

            var stringBuffer = new byte[NameLen];
            stream.Read(stringBuffer, 0, NameLen);
            Name = System.Text.ASCIIEncoding.ASCII.GetString(stringBuffer);

            //if the endoffset is 0 the node is null
            if (EndOffset == 0)
            {
                IsNull = true;
                return;
            }

            //read properties
            for(var i = 0; i < NumProperties; i++)
            {
                Properties.Add(new Property(stream));
            }

            // read nested nodes
            while (stream.Position < EndOffset)
            {
                var node = new FBXFileNode(stream);

                // the null node marks the end of nested nodes
                if (node.IsNull)
                    return;

                Nodes.Add(node);
            }
        }

        public string GetStructure(int padding)
        {
            var pad = new string(' ', padding);
            var result = pad + Name;

            var props = "";

            foreach (var prop in Properties)
                props += (string.IsNullOrEmpty(props) ? ": " : ", ") + "\"" + (prop.Data != null ? prop.Data.ToString() : "null") + "\"";

            result += props;

            foreach (var node in Nodes)
                result += "\r\n" + node.GetStructure(padding + 2);

            return result;
        }

        public FBXFileNode FindChild(long id)
        {
            foreach (var node in Nodes)
            {
                if (node.Id == id)
                    return node;

                var found = node.FindChild(id);

                if (found != null)
                    return found;
            }

            return null;
        }
    }
}
