using CoreMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreFBX.FBX
{
    public class Model
    {
        public long Id { get; set; } = 0;
        public string Name { get; set; } = "";
        public FBXModelType ModelType { get; set; } = FBXModelType.None;

        public float[] LclScaling { get; set; } = new float[]
        {
            1, 1, 1
        };

        public float[] LclRotation { get; set; } = new float[]
        {
            0, 0, 0
        };

        public float[] LclTranslation { get; set; } = new float[]
        {
            0, 0, 0
        };

        /// <summary>
        /// This transform shouldn't be applied to child nodes
        /// </summary>
        public float[] GeometricScaling { get; set; } = new float[]
        {
            1, 1, 1
        };

        /// <summary>
        /// This transform shouldn't be applied to child nodes
        /// </summary>
        public float[] GeometricRotation { get; set; } = new float[]
        {
            0, 0, 0
        };

        /// <summary>
        /// This transform shouldn't be applied to child nodes
        /// </summary>
        public float[] GeometricTranslation { get; set; } = new float[]
        {
            0, 0, 0
        };

        public float[] BindMatrix { get; set; } = new float[]
        {
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        };

        public float[] PreRotation { get; set; } = new float[]
        {
            0, 0, 0
        };

        public float[] PostRotation { get; set; } = new float[]
        {
            0, 0, 0
        };

        public float[] Rotation
        {
            get
            {
                return PostRotation.ToRadians().QuaternionFromEuler(EulerOrder.XYZ).QuaternionProduct(LclRotation.ToRadians().QuaternionFromEuler(EulerOrder.XYZ).QuaternionProduct(PreRotation.ToRadians().QuaternionFromEuler(EulerOrder.XYZ)));
            }
        }

        public float[] Transform
        {
            get
            {
                return (new float[] { }).MatrixCompose(LclTranslation, Rotation, GeometricScaling);
            }
        }

        public InheritTypeOption InheritType { get; set; } = InheritTypeOption.eInheritRSrs;

        /// <summary>
        /// How to apply child transformations in parent space
        /// </summary>
        public enum InheritTypeOption : int
        {
            /// <summary>
            /// Parent Rotation - Child Rotation - Parent Scale - Child Scale
            /// </summary>
            eInheritRrSs,
            /// <summary>
            /// Parent Rotation - Parent Scale - Child Rotation - Child Scale
            /// </summary>
            eInheritRSrs,
            /// <summary>
            /// Parent Rotation - Child rotation - Child Scale
            /// </summary>
            eInheritRrs
        }

        public bool HasGeometry { get; set; }

        public Model() { }

        public Model(FBXFileNode node, FBXFile fbx)
        {
            Id = node.Id;
            Name = ((string)node.Properties[1].Data);

            HasGeometry = node.Nodes.Where(a => a.Name == "Geometry").Count() > 0;

            switch ((string)node.Properties[2].Data)
            {
                case "Mesh":
                    ModelType = FBXModelType.Mesh;
                    break;
                case "LimbNode":
                    ModelType = FBXModelType.LimbNode;
                    break;
            }

            var propNode = node.Nodes.Where(a => a.Name == "Properties70").FirstOrDefault();
            var rotationActive = true;

           // if(fbx.GlobalSettings != null && fbx.GlobalSettings.UnitScaleFactor != 0)
           //     Scaling = Scaling.SetScalingPart((float)fbx.GlobalSettings.UnitScaleFactor, (float)fbx.GlobalSettings.UnitScaleFactor, (float)fbx.GlobalSettings.UnitScaleFactor);

            if (propNode != null)
            {
                foreach (var child in propNode.Nodes)
                {
                    if (child.Name == "P")
                    {
                        var property = (string)child.Properties[0].Data;

                        switch (property)
                        {
                            case "PreRotation":
                                PreRotation = new float[]
                                {
                                    (float)(double)child.Properties[4].Data,
                                    (float)(double)child.Properties[5].Data,
                                    (float)(double)child.Properties[6].Data
                                };
                                break;
                            case "Lcl Rotation":
                                LclRotation = new float[]
                                {
                                        (float)(double)child.Properties[4].Data,
                                        (float)(double)child.Properties[5].Data,
                                        (float)(double)child.Properties[6].Data
                                };
                                 break;
                            case "RotationActive":
                                rotationActive = (int)child.Properties[4].Data == 1;
                                break;
                            case "Lcl Translation":
                                LclTranslation = new float[]
                                {
                                        (float)(double)child.Properties[4].Data,
                                        (float)(double)child.Properties[5].Data,
                                        (float)(double)child.Properties[6].Data
                                };
                                break;
                            case "Lcl Scaling":
                                LclScaling = new float[]
                                {
                                        (float)(double)child.Properties[4].Data,
                                        (float)(double)child.Properties[5].Data,
                                        (float)(double)child.Properties[6].Data
                                };
                                break;
                            case "PostRotation":
                                PostRotation = new float[]
                                {
                                        (float)(double)child.Properties[4].Data,
                                        (float)(double)child.Properties[5].Data,
                                        (float)(double)child.Properties[6].Data
                                };
                                break;
                            case "GeometricTranslation":
                                GeometricTranslation = new float[]
                                {
                                        (float)(double)child.Properties[4].Data,
                                        (float)(double)child.Properties[5].Data,
                                        (float)(double)child.Properties[6].Data
                                };
                                break;
                            case "GeometricScaling":
                                GeometricScaling = new float[]
                                {
                                        (float)(double)child.Properties[4].Data,
                                        (float)(double)child.Properties[5].Data,
                                        (float)(double)child.Properties[6].Data
                                };
                                break;
                            case "GeometricRotation ":
                                GeometricRotation = new float[]
                                {
                                        (float)(double)child.Properties[4].Data,
                                        (float)(double)child.Properties[5].Data,
                                        (float)(double)child.Properties[6].Data
                                };
                                break;
                            case "InheritType":
                                InheritType = (InheritTypeOption)child.Properties[4].Data;
                                break;
                        }
                    }
                }
            }
        }

        public enum FBXModelType
        {
            None,
            Mesh,
            LimbNode
        }
    }
}
