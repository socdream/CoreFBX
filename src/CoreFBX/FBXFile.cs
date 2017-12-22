using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreFBX.FBX
{
    /// <summary>
    /// https://github.com/hamish-milne/FbxWriter
    /// </summary>
    public class FBXFile
    {
        // Header string, found at the top of all compliant files
        private static readonly byte[] headerString
            = Encoding.ASCII.GetBytes("Kaydara FBX Binary  \0\x1a\0");

        // This data was entirely calculated by me, honest. Turns out it works, fancy that!
        private static readonly byte[] sourceId =
            { 0x58, 0xAB, 0xA9, 0xF0, 0x6C, 0xA2, 0xD8, 0x3F, 0x4D, 0x47, 0x49, 0xA3, 0xB4, 0xB2, 0xE7, 0x3D };
        private static readonly byte[] key =
            { 0xE2, 0x4F, 0x7B, 0x5F, 0xCD, 0xE4, 0xC8, 0x6D, 0xDB, 0xD8, 0xFB, 0xD7, 0x40, 0x58, 0xC6, 0x78 };
        // This wasn't - it just appears at the end of every compliant file
        private static readonly byte[] extension =
            { 0xF8, 0x5A, 0x8C, 0x6A, 0xDE, 0xF5, 0xD9, 0x7E, 0xEC, 0xE9, 0x0C, 0xE3, 0x75, 0x8F, 0x29, 0x0B };

        // Number of null bytes between the footer code and the version
        private const int footerZeroes1 = 20;
        // Number of null bytes between the footer version and extension code
        private const int footerZeroes2 = 120;

        /// <summary>
		/// The size of the footer code
		/// </summary>
		protected const int footerCodeSize = 16;

        public uint Version { get; set; }

        public List<FBXFileNode> Nodes { get; set; } = new List<FBXFileNode>();

        public FBXFile(Stream stream)
        {
            // The first 27 bytes contain the header
            var valid = ReadHeader(stream);

            if (!valid)
                throw new Exception("Invalid header string.");
            
            //read the version
            var uintBuffer = new byte[4];
            stream.Read(uintBuffer, 0, 4);
            Version = BitConverter.ToUInt32(uintBuffer, 0);

            // Read nodes
            while(stream.Position < stream.Length)
            {
                var node = new FBXFileNode(stream);

                if (node.IsNull)
                    break;

                Nodes.Add(node);
            }

            // read the footer code
            var footerCode = new byte[footerCodeSize];
            stream.Read(footerCode, 0, footerCode.Length);

            var validCode = GenerateFooterCode();
            if (!CheckEqual(footerCode, validCode))
                throw new Exception("Incorrect footer code");

            // Read footer extension
            var validFooterExtension = CheckFooter(stream);

            /*if (!validFooterExtension)
                throw new Exception("Invalid footer");*/
        }

        /// <summary>
		/// Writes the FBX header string
		/// </summary>
		/// <param name="stream"></param>
		private void WriteHeader(Stream stream)
        {
            stream.Write(headerString, 0, headerString.Length);
        }

        /// <summary>
		/// Reads the FBX header string
		/// </summary>
		/// <param name="stream"></param>
		/// <returns><c>true</c> if it's compliant</returns>
		private bool ReadHeader(Stream stream)
        {
            var buf = new byte[headerString.Length];
            stream.Read(buf, 0, buf.Length);
            return CheckEqual(headerString, buf);
        }

        // Turns out this is the algorithm they use to generate the footer. Who knew!
        static void Encrypt(byte[] a, byte[] b)
        {
            byte c = 64;
            for (int i = 0; i < footerCodeSize; i++)
            {
                a[i] = (byte)(a[i] ^ (byte)(c ^ b[i]));
                c = a[i];
            }
        }

        const string timePath1 = "FBXHeaderExtension";
        const string timePath2 = "CreationTimeStamp";
        static readonly Stack<string> timePath = new Stack<string>(new[] { timePath1, timePath2 });

        // Gets a single timestamp component
        static int GetTimestampVar(FBXFileNode timestamp, string element)
        {
            var elementNode = timestamp.Nodes.Where(a => a.Name == element).FirstOrDefault();
            if (elementNode != null && elementNode.Properties.Count > 0)
            {
                var prop = elementNode.Properties[0];
                
                if (prop.Data is int || prop.Data is long)
                    return (int)prop.Data;
            }
            throw new Exception("Timestamp has no " + element);
        }

        /// <summary>
        /// Generates the unique footer code based on the document's timestamp
        /// </summary>
        /// <param name="document"></param>
        /// <returns>A 16-byte code</returns>
        private byte[] GenerateFooterCode()
        {
            var parentNode = Nodes.Where(a => a.Name == timePath1).FirstOrDefault();

            if(parentNode == null)
                throw new Exception("No creation timestamp");

            var timeNode = parentNode.Nodes.Where(a => a.Name == timePath2).FirstOrDefault();

            if (timeNode == null)
                throw new Exception("No creation timestamp");
            
            try
            {
                return GenerateFooterCode(
                    GetTimestampVar(timeNode, "Year"),
                    GetTimestampVar(timeNode, "Month"),
                    GetTimestampVar(timeNode, "Day"),
                    GetTimestampVar(timeNode, "Hour"),
                    GetTimestampVar(timeNode, "Minute"),
                    GetTimestampVar(timeNode, "Second"),
                    GetTimestampVar(timeNode, "Millisecond")
                    );
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new Exception("Invalid timestamp");
            }
        }

        /// <summary>
        /// Generates a unique footer code based on a timestamp
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        /// <param name="millisecond"></param>
        /// <returns>A 16-byte code</returns>
        protected static byte[] GenerateFooterCode(
            int year, int month, int day,
            int hour, int minute, int second, int millisecond)
        {
            if (year < 0 || year > 9999)
                throw new ArgumentOutOfRangeException(nameof(year));
            if (month < 0 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month));
            if (day < 0 || day > 31)
                throw new ArgumentOutOfRangeException(nameof(day));
            if (hour < 0 || hour >= 24)
                throw new ArgumentOutOfRangeException(nameof(hour));
            if (minute < 0 || minute >= 60)
                throw new ArgumentOutOfRangeException(nameof(minute));
            if (second < 0 || second >= 60)
                throw new ArgumentOutOfRangeException(nameof(second));
            if (millisecond < 0 || millisecond >= 1000)
                throw new ArgumentOutOfRangeException(nameof(millisecond));

            var str = (byte[])sourceId.Clone();
            var mangledTime = $"{second:00}{month:00}{hour:00}{day:00}{(millisecond / 10):00}{year:0000}{minute:00}";
            var mangledBytes = Encoding.ASCII.GetBytes(mangledTime);
            Encrypt(str, mangledBytes);
            Encrypt(str, key);
            Encrypt(str, mangledBytes);
            return str;
        }

        /// <summary>
        /// Writes the FBX footer extension (NB - not the unique footer code)
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="version"></param>
        protected void WriteFooter(BinaryWriter stream, int version)
        {
            var zeroes = new byte[Math.Max(footerZeroes1, footerZeroes2)];
            stream.Write(zeroes, 0, footerZeroes1);
            stream.Write(version);
            stream.Write(zeroes, 0, footerZeroes2);
            stream.Write(extension, 0, extension.Length);
        }

        static bool AllZero(byte[] array)
        {
            foreach (var b in array)
                if (b != 0)
                    return false;
            return true;
        }

        /// <summary>
        /// Checks if the first part of 'data' matches 'original'
        /// </summary>
        /// <param name="data"></param>
        /// <param name="original"></param>
        /// <returns><c>true</c> if it does, otherwise <c>false</c></returns>
        protected static bool CheckEqual(byte[] data, byte[] original)
        {
            for (int i = 0; i < original.Length; i++)
                if (data[i] != original[i])
                    return false;
            return true;
        }

        /// <summary>
        /// Reads and checks the FBX footer extension (NB - not the unique footer code)
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="version"></param>
        /// <returns><c>true</c> if it's compliant</returns>
        protected bool CheckFooter(Stream stream)
        {
            var buffer = new byte[Math.Max(footerZeroes1, footerZeroes2)];
            stream.Read(buffer, 0, footerZeroes1);
            bool correct = AllZero(buffer);

            var vBuffer = new byte[4];
            stream.Read(vBuffer, 0, vBuffer.Length);
            var readVersion = BitConverter.ToInt32(vBuffer, 0);

            correct &= (readVersion == (int)Version);
            stream.Read(buffer, 0, footerZeroes2);
            correct &= AllZero(buffer);
            stream.Read(buffer, 0, extension.Length);
            correct &= CheckEqual(buffer, extension);
            return correct;
        }

        public string GetStructure()
        {
            var result = "";
            
            foreach (var node in Nodes)
                result += "\r\n" + node.GetStructure(0);

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

        private List<Connection> _connections;

        public List<Connection> Connections
        {
            get
            {
                if (_connections != null)
                    return _connections;

                _connections = new List<FBX.Connection>();

                var conNodes = Nodes.Where(a => a.Name == "Connections").FirstOrDefault()
                        .Nodes.Where(a => a.Name == "C");

                foreach (var node in conNodes)
                {
                    _connections.Add(new FBX.Connection(node));
                }

                return _connections;
            }
        }

        private List<PoseNode> _poseNodes;

        public List<PoseNode> PoseNodes
        {
            get
            {
                if (_poseNodes != null)
                    return _poseNodes;

                _poseNodes = new List<FBX.PoseNode>();

                // load bind pose matrices for each model
                var poseParent = Nodes.Where(a => a.Name == "Objects").FirstOrDefault()
                        .Nodes.Where(a => a.Name == "Pose").FirstOrDefault();

                if (poseParent != null)
                {
                    var poseNodes = poseParent.Nodes.Where(a => a.Name == "PoseNode");

                    foreach (var node in poseNodes)
                        _poseNodes.Add(new PoseNode(node, poseParent));
                }

                return _poseNodes;
            }
        }

        private GlobalSettings _globalSettings;

        public GlobalSettings GlobalSettings
        {
            get
            {
                if(_globalSettings == null)
                    _globalSettings = new GlobalSettings(this);

                return _globalSettings;
            }
        }

        private FBXTreeNode _rootNode;

        public FBXTreeNode RootNode
        {
            get
            {
                if (_rootNode != null)
                    return _rootNode;

                _rootNode = new FBXTreeNode(null, this);

                return _rootNode;
            }
        }

        public List<Deformer> GetClusters(FBXTreeNode geometryNode)
        {
            var deformer = geometryNode.Children.Where(a => a.Node.Name == "Deformer").FirstOrDefault();
            var clusters = new List<FBX.Deformer>();

            if (deformer != null)
            {
                var subdeformersTree = deformer.Children.Where(a => a.Node.Name == "Deformer");

                foreach (var tempNode in subdeformersTree)
                    clusters.Add(FBX.Deformer.FromTreeNode(tempNode, PoseNodes));
            }

            return clusters;
        }

        public List<Animation.FBXAnimStack> GetAnimationStacks(FBXFileNode root = null)
        {
            var result = new List<Animation.FBXAnimStack>();
            var nodes = (root == null) ? Nodes : root.Nodes;

            foreach (var node in nodes)
            {
                if (node.Name == "AnimationStack")
                    result.Add(new Animation.FBXAnimStack(node));
                
                result.AddRange(GetAnimationStacks(node));
            }

            return result;
        }

        public List<Animation.FBXAnimCurve> GetAnimationCurves(FBXFileNode root = null)
        {
            var result = new List<Animation.FBXAnimCurve>();
            var nodes = (root == null) ? Nodes : root.Nodes;

            foreach (var node in nodes)
            {
                if (node.Name == "AnimationCurve")
                    result.Add(new Animation.FBXAnimCurve(node));

                result.AddRange(GetAnimationCurves(node));
            }

            return result;
        }

        public List<Animation.FBXAnimCurve> GetAnimationCurves(Animation.FBXAnimCurveNode root)
        {
            var result = new List<Animation.FBXAnimCurve>();

            var nodes = RootNode.FindChild(root.Id).Children;

            foreach (var node in nodes)
            {
                if (node.Node.Name == "AnimationCurve")
                    result.Add(new Animation.FBXAnimCurve(node.Node));
            }

            return result;
        }

        public List<Animation.FBXAnimCurveNode> GetAnimationCurveNodes(FBXFileNode root = null)
        {
            var result = new List<Animation.FBXAnimCurveNode>();
            var nodes = (root == null) ? Nodes : root.Nodes;

            foreach (var node in nodes)
            {
                if (node.Name == "AnimationCurveNode")
                    result.Add(new Animation.FBXAnimCurveNode(node));

                result.AddRange(GetAnimationCurveNodes(node));
            }

            return result;
        }

        public string GetConnectionType(long child, long parent)
        {
            var connection = Connections.Where(a => a.Src == child && a.Dst == parent).FirstOrDefault();

            if (connection != null)
                return connection.ConnectionType;

            return null;
        }
    }
}
