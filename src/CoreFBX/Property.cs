using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreFBX.FBX
{
    public class Property
    {
        public PropertyType TypeCode { get; set; }
        public object Data { get; set; }

        /// <summary>
        /// The namespace separator in the binary format (remember to reverse the identifiers)
        /// </summary>
        protected const string binarySeparator = "\0\x1";

        /// <summary>
        /// The namespace separator in the ASCII format and in object data
        /// </summary>
        protected const string asciiSeparator = "::";

        public Property(Stream stream)
        {
            TypeCode = (PropertyType)stream.ReadByte();
            var buffer = new byte[0];

            switch (TypeCode)
            {
                case PropertyType.Short:
                    buffer = new byte[2];
                    stream.Read(buffer, 0, buffer.Length);
                    Data = BitConverter.ToInt16(buffer, 0);
                    break;
                case PropertyType.Bool:
                    Data = stream.ReadByte() == 1;
                    break;
                case PropertyType.Int:
                    buffer = new byte[4];
                    stream.Read(buffer, 0, buffer.Length);
                    Data = BitConverter.ToInt32(buffer, 0);
                    break;
                case PropertyType.Float:
                    buffer = new byte[4];
                    stream.Read(buffer, 0, buffer.Length);
                    Data = BitConverter.ToSingle(buffer, 0);
                    break;
                case PropertyType.Double:
                    buffer = new byte[8];
                    stream.Read(buffer, 0, buffer.Length);
                    Data = BitConverter.ToDouble(buffer, 0);
                    break;
                case PropertyType.Long:
                    buffer = new byte[8];
                    stream.Read(buffer, 0, buffer.Length);
                    Data = BitConverter.ToInt64(buffer, 0);
                    break;
                case PropertyType.BoolArray:
                case PropertyType.DoubleArray:
                case PropertyType.FloatArray:
                case PropertyType.IntArray:
                case PropertyType.LongArray:
                    ReadArray(stream);
                    break;
                case PropertyType.String:
                    buffer = new byte[4];
                    stream.Read(buffer, 0, buffer.Length);
                    var length = BitConverter.ToUInt32(buffer, 0);
                    buffer = new byte[length];
                    stream.Read(buffer, 0, buffer.Length);
                    var str = System.Text.Encoding.ASCII.GetString(buffer, 0, buffer.Length);
                    
                    // Convert \0\1 to '::' and reverse the tokens
                    if (str.Contains(binarySeparator))
                    {
                        var tokens = str.Split(new[] { binarySeparator }, StringSplitOptions.None);
                        var sb = new StringBuilder();
                        bool first = true;
                        for (int i = tokens.Length - 1; i >= 0; i--)
                        {
                            if (!first)
                                sb.Append(asciiSeparator);
                            sb.Append(tokens[i]);
                            first = false;
                        }
                        str = sb.ToString();
                    }

                    Data = str;

                    break;
                case PropertyType.Raw:
                    buffer = new byte[4];
                    stream.Read(buffer, 0, buffer.Length);
                    var lengthB = BitConverter.ToUInt32(buffer, 0);
                    buffer = new byte[lengthB];
                    stream.Read(buffer, 0, buffer.Length);
                    Data = buffer;
                    break;
            }
        }
        
        private void ReadArray(Stream stream)
        {
            var buffer = new byte[4];
            stream.Read(buffer, 0, buffer.Length);
            var arrayLength = BitConverter.ToUInt32(buffer, 0);

            stream.Read(buffer, 0, buffer.Length);
            var encoding = BitConverter.ToUInt32(buffer, 0);

            stream.Read(buffer, 0, buffer.Length);
            var compressedLength = BitConverter.ToUInt32(buffer, 0);

            var endPos = stream.Position + compressedLength;

            Stream s = stream;

            // check if the data is compressed
            if(encoding != 0)
            {
                if (encoding != 1)
                    throw new Exception("Invalid compression encoding (must be 0 or 1)");

                var cmf = stream.ReadByte();

                if ((cmf & 0xF) != 8 || (cmf >> 4) > 7)
                    throw new Exception("Invalid compression format " + cmf);

                var flg = stream.ReadByte();

                if (((cmf << 8) + flg) % 31 != 0)
                    throw new Exception("Invalid compression FCHECK");

                if ((flg & (1 << 5)) != 0)
                    throw new Exception("Invalid compression flags; dictionary not supported");

                s = new DeflateWithChecksum(stream, CompressionMode.Decompress);
            }

            switch (TypeCode)
            {
                case PropertyType.BoolArray:
                    buffer = new byte[arrayLength];
                    s.Read(buffer, 0, buffer.Length);
                    Data = buffer.Select(a => a == 1).ToArray();
                    break;
                case PropertyType.DoubleArray:
                    buffer = new byte[arrayLength * 8];
                    s.Read(buffer, 0, buffer.Length);
                    var result = new double[arrayLength];
                    Buffer.BlockCopy(buffer, 0, result, 0, buffer.Length);
                    Data = result;
                    break;
                case PropertyType.FloatArray:
                    buffer = new byte[arrayLength * 4];
                    s.Read(buffer, 0, buffer.Length);
                    var resultF = new float[arrayLength];
                    Buffer.BlockCopy(buffer, 0, resultF, 0, buffer.Length);
                    Data = resultF;
                    break;
                case PropertyType.IntArray:
                    buffer = new byte[arrayLength * 4];
                    s.Read(buffer, 0, buffer.Length);
                    var resultI = new int[arrayLength];
                    Buffer.BlockCopy(buffer, 0, resultI, 0, buffer.Length);
                    Data = resultI;
                    break;
                case PropertyType.LongArray:
                    buffer = new byte[arrayLength * 8];
                    s.Read(buffer, 0, buffer.Length);
                    var resultL = new long[arrayLength];
                    Buffer.BlockCopy(buffer, 0, resultL, 0, buffer.Length);
                    Data = resultL;
                    break;
            }

            stream.Position = endPos;
        }

        public enum PropertyType : byte
        {
            Short = (byte)'Y',
            Bool = (byte)'C',
            Int = (byte)'I',
            Float = (byte)'F',
            Double = (byte)'D',
            Long = (byte)'L',
            FloatArray = (byte)'f',
            DoubleArray = (byte)'d',
            LongArray = (byte)'l',
            IntArray = (byte)'i',
            BoolArray = (byte)'b',
            String = (byte)'S',
            Raw = (byte)'R'
        }
    }
}
