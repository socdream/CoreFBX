using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreFBX.FBX
{
    public class GlobalSettings
    {
        public int Version { get; set; }
        public int UpAxis { get; set; }
        public int UpAxisSign { get; set; }
        public int FrontAxis { get; set; }
        public int FrontAxisSign { get; set; }
        public int CoordAxis { get; set; }
        public int CoordAxisSign { get; set; }
        public int OriginalUpAxis { get; set; }
        public int OriginalUpAxisSign { get; set; }
        public double OriginalUnitScaleFactor { get; set; }
        public TimeModeOption TimeMode { get; set; }
        public double CustomFrameRate { get; set; }
        public double UnitScaleFactor { get; set; }

        public GlobalSettings(FBXFile fbx)
        {
            var globalNode = fbx.Nodes.Where(a => a.Name == "GlobalSettings").FirstOrDefault();

            if(globalNode != null)
            {
                var versionNode = globalNode.Nodes.Where(a => a.Name == "Version").FirstOrDefault();

                if(versionNode != null)
                {
                    Version = (int)versionNode.Properties[0].Data;
                }

                var propNode = globalNode.Nodes.Where(a => a.Name == "Properties70").FirstOrDefault();

                if (propNode != null)
                {
                    foreach (var child in propNode.Nodes)
                    {
                        if (child.Name == "P")
                        {
                            var property = (string)child.Properties[0].Data;

                            switch (property)
                            {
                                case "UpAxis":
                                    UpAxis = (int)child.Properties[4].Data;
                                    break;
                                case "UpAxisSign":
                                    UpAxisSign = (int)child.Properties[4].Data;
                                    break;
                                case "FrontAxis":
                                    FrontAxis = (int)child.Properties[4].Data;
                                    break;
                                case "FrontAxisSign":
                                    FrontAxisSign = (int)child.Properties[4].Data;
                                    break;
                                case "CoordAxis":
                                    CoordAxis = (int)child.Properties[4].Data;
                                    break;
                                case "CoordAxisSign":
                                    CoordAxisSign = (int)child.Properties[4].Data;
                                    break;
                                case "OriginalUpAxis":
                                    OriginalUpAxis = (int)child.Properties[4].Data;
                                    break;
                                case "OriginalUpAxisSign":
                                    OriginalUpAxisSign = (int)child.Properties[4].Data;
                                    break;
                                case "UnitScaleFactor":
                                    UnitScaleFactor = (double)child.Properties[4].Data;
                                    break;
                                case "OriginalUnitScaleFactor":
                                    OriginalUnitScaleFactor = (double)child.Properties[4].Data;
                                    break;
                                case "TimeMode":
                                    TimeMode = (TimeModeOption)child.Properties[4].Data;
                                    break;
                                case "CustomFrameRate":
                                    CustomFrameRate = (double)child.Properties[4].Data;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        public enum TimeModeOption : int
        {
            eDefaultMode,
            eFrames120,
            eFrames100,
            eFrames60,
            eFrames50,
            eFrames48,
            eFrames30,
            eFrames30Drop,
            eNTSCDropFrame,
            eNTSCFullFrame,
            ePAL,
            eFrames24,
            eFrames1000,
            eFilmFullFrame,
            eCustom,
            eFrames96,
            eFrames72,
            eFrames59dot94,
            eModesCount
        }
    }
}
