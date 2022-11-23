using System;
using System.Collections.Generic;
using VLive.Runtime.MediaPipe;
using VRM;
namespace VLive.Runtime.Extensions
{
    public static class BlendShapeExtensions
    {
        private static Dictionary<FaceBlendShape, (float min, float max)> _blendShapeMinMaxDict = new()
        {
            // {
            //     FaceBlendShape.EyeBlinkLeft, (0.40f, 0.70f)
            // },
            // {
            //     FaceBlendShape.EyeSquintLeft, (0.37f, 0.44f)
            // },
            // {
            //     FaceBlendShape.EyeWideLeft, (0.9f, 1.2f)
            // },
            // {
            //     FaceBlendShape.EyeBlinkRight, (0.40f, 0.70f)
            // },
            // {
            //     FaceBlendShape.EyeSquintRight, (0.37f, 0.44f)
            // },
            // {
            //     FaceBlendShape.EyeWideRight, (0.9f, 1.2f)
            // },
            // {FaceBlendShape.JawLeft, (-0.4f, 0.0f)},
            // {FaceBlendShape.JawRight, (0.0f, 0.4f)},
            // {FaceBlendShape.JawOpen, (0.45f, 0.55f)},
            {FaceBlendShape.MouthClose, (0, 0.1f)},
            // {FaceBlendShape.MouthFunnel, (4.0f, 4.8f)},
            // {FaceBlendShape.MouthPucker, (3.46f, 4.92f)},
            // {FaceBlendShape.MouthLeft, (-3.4f, -2.3f)},
            // {FaceBlendShape.MouthRight, (1.5f, 3.0f)},
            // {FaceBlendShape.MouthSmileLeft, (-0.01f, 0.002f)},
            // {FaceBlendShape.MouthSmileRight, (-0.01f, 0.002f)},
            // {FaceBlendShape.MouthFrownLeft, (0.4f, 0.9f)},
            // {FaceBlendShape.MouthFrownRight, (0.4f, 0.9f)},
            // {FaceBlendShape.MouthStretchLeft, (-0.02f, -0.001f)},
            // {FaceBlendShape.MouthStretchRight, (0.001f, 0.02f)},
            // {FaceBlendShape.MouthRollLower, (0.4f, 0.7f)},
            // {FaceBlendShape.MouthRollUpper, (0.31f, 0.34f)},
            // {FaceBlendShape.MouthShrugLower, (1.9f, 2.3f)},
            // {FaceBlendShape.MouthShrugUpper, (1.4f, 2.4f)},
            // {FaceBlendShape.MouthPressLeft, (0.4f, 0.5f)},
            // {FaceBlendShape.MouthPressRight, (0.4f, 0.5f)},
            // {FaceBlendShape.MouthLowerDownLeft, (1.7f, 2.1f)},
            // {FaceBlendShape.MouthLowerDownRight, (1.7f, 2.1f)},
            // {FaceBlendShape.BrowDownLeft, (1.0f, 1.2f)},
            // {FaceBlendShape.BrowDownRight, (1.0f, 1.2f)},
            // {FaceBlendShape.BrowInnerUp, (2.2f, 2.6f)},
            // {FaceBlendShape.BrowOuterUpLeft, (1.25f, 1.5f)},
            // {FaceBlendShape.BrowOuterUpRight, (1.25f, 1.5f)},
            // {FaceBlendShape.CheekSquintLeft, (0.55f, 0.63f)},
            // {FaceBlendShape.CheekSquintRight, (0.55f, 0.63f)},
            {FaceBlendShape.MouthStretch, (0.045f, 0.075f)}
        };

        public static float RemapBlendShape(this FaceBlendShape shape, float value)
        {
            _blendShapeMinMaxDict.TryGetValue(shape, out var tuple);
            return tuple == default ? value : value.Remap(tuple.min, tuple.max);
        }

        public static void Set(this FaceBlendShape shape, VRMBlendShapeProxy proxy, float value, bool remap = false)
        {
            if (remap)
            {
                value = shape.RemapBlendShape(value);
            }
            proxy.SetValue(shape, value);
        }

        private static void SetValue(this VRMBlendShapeProxy proxy, FaceBlendShape shape, float value)
        {
            proxy.ImmediatelySetValue(BlendShapeKey.CreateUnknown(shape.ToString()), value);
        }
        
        public static void AccumulateValue(this VRMBlendShapeProxy proxy, FaceBlendShape shape, float value)
        {
            proxy.AccumulateValue(BlendShapeKey.CreateUnknown(shape.ToString()), value);
        }
    }
}
