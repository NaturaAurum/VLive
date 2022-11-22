using UnityEngine;
using VRM;
namespace VLive.Runtime
{
    public enum FaceBlendShape
    {
        EyeBlinkLeft = 0,
        EyeLookDownLeft = 1,
        EyeLookInLeft = 2,
        EyeLookOutLeft = 3,
        EyeLookUpLeft = 4,
        EyeSquintLeft = 5,
        EyeWideLeft = 6,
        EyeBlinkRight = 7,
        EyeLookDownRight = 8,
        EyeLookInRight = 9,
        EyeLookOutRight = 10,
        EyeLookUpRight = 11,
        EyeSquintRight = 12,
        EyeWideRight = 13,
        JawForward = 14,
        JawLeft = 15,
        JawRight = 16,
        JawOpen = 17,
        MouthClose = 18,
        MouthFunnel = 19,
        MouthPucker = 20,
        MouthLeft = 21,
        MouthRight = 22,
        MouthSmileLeft = 23,
        MouthSmileRight = 24,
        MouthFrownLeft = 25,
        MouthFrownRight = 26,
        MouthDimpleLeft = 27,
        MouthDimpleRight = 28,
        MouthStretchLeft = 29,
        MouthStretchRight = 30,
        MouthRollLower = 31,
        MouthRollUpper = 32,
        MouthShrugLower = 33,
        MouthShrugUpper = 34,
        MouthPressLeft = 35,
        MouthPressRight = 36,
        MouthLowerDownLeft = 37,
        MouthLowerDownRight = 38,
        MouthUpperUpLeft = 39,
        MouthUpperUpRight = 40,
        BrowDownLeft = 41,
        BrowDownRight = 42,
        BrowInnerUp = 43,
        BrowOuterUpLeft = 44,
        BrowOuterUpRight = 45,
        CheekPuff = 46,
        CheekSquintLeft = 47,
        CheekSquintRight = 48,
        NoseSneerLeft = 49,
        NoseSneerRight = 50,
        TongueOut = 51,
        HeadYaw = 52,
        HeadPitch = 53,
        HeadRoll = 54,
        LeftEyeYaw = 55,
        LeftEyePitch = 56,
        LeftEyeRoll = 57,
        RightEyeYaw = 58,
        RightEyePitch = 59,
        RightEyeRoll = 60
    }

    public class FaceSolver : MonoBehaviour, IFaceSolver
    {
        private struct CanonicalPoints
        {
            public static int[] EyeRight = {33, 133, 160, 159, 158, 144, 145, 153};
            public static int[] EyeLeft = {263, 362, 387, 386, 385, 373, 374, 380};
            public static int[] Head = {10, 152};
            public const int NoseTip = 1;
            public const int UpperLip = 13;
            public const int LowerLip = 14;
            public const int UpperOuterLip = 12;
            public const int MouthCornerLeft = 291;
            public const int MouthCornerRight = 61;
            public const int LowestChin = 152;
            public const int UpperHead = 10;
            public const int MouthFrownLeft = 422;
            public const int MouthFrownRight = 202;
            public const int MouthLeftStretch = 287;
            public const int MouthRightStretch = 57;
            public const int LowestLip = 17;
            public const int UnderLip = 18;
            public const int OverUpperLip = 164;
            public static int[] LeftUpperPress = {40, 80};
            public static int[] LeftLowerPress = {88, 91};
            public static int[] RightUpperPress = {270, 310};
            public static int[] RightLowerPress = {318, 321};
            public static int[] SquintLeft = {253, 450};
            public static int[] SquintRight = {23, 230};
            public const int RightBrow = 27;
            public static int[] RightBrowLower = {53, 52, 65};
            public const int LeftBrow = 257;
            public static int[] LeftBrowLower = {283, 282, 295};
            public const int InnerBrow = 9;
            public const int UpperNose = 6;
            public static int[] CheckSquintLeft = {359, 342};
            public static int[] CheckSquintRight = {130, 113};
        }

        private VRMBlendShapeProxy _blendShapeProxy;

        private bool _isPerfectSync = false;

        private void Awake()
        {
            _blendShapeProxy = GetComponent<VRMBlendShapeProxy>();

            var blendShapeAvatar = _blendShapeProxy.BlendShapeAvatar;
            // 대충 52개 이상이면 있는걸로
            _isPerfectSync = blendShapeAvatar.Clips.Count >= 52;
        }

        public void Solve(FaceData data)
        {
            if (data.PosList is {Count: < 451})
            {
                return;
            }
            
            MouthSolve(data);
            EyeSolve(data);
            _blendShapeProxy.Apply();
        }
        
       

        private void HeadRotationSolve(FaceData data)
        {
            
        }

        private void MouthSolve(FaceData data)
        {
            if (_isPerfectSync)
            {
                PerfectMouthSolve(data);
            }
            else
            {
                // TODO
            }
        }

        private void PerfectMouthSolve(FaceData data)
        {
            var upperLip = data.PosList[CanonicalPoints.UpperLip].Point;
            var upperOuterLip = data.PosList[CanonicalPoints.UpperOuterLip].Point;
            var lowerLip = data.PosList[CanonicalPoints.LowerLip].Point;
            
            var mouthCornerLeft = data.PosList[CanonicalPoints.MouthCornerLeft].Point;
            var mouthCornerRight = data.PosList[CanonicalPoints.MouthCornerRight].Point;
            var lowestChin = data.PosList[CanonicalPoints.LowestChin].Point;
            var noseTip = data.PosList[CanonicalPoints.NoseTip].Point;
            var upperHead = data.PosList[CanonicalPoints.UpperHead].Point;

            var mouthWidth = Vector3.Distance(mouthCornerLeft, mouthCornerRight);
            var mouthCenter = (upperLip + lowerLip) * 0.5f;
            var mouthOpenDist = Vector3.Distance(upperLip, lowerLip);
            var mouthCenterNoseDist = Vector3.Distance(mouthCenter, noseTip);

            var jawNoseDist = Vector3.Distance(lowestChin, noseTip);
            var headHeight = Vector3.Distance(upperHead, lowestChin);
            var jawOpenRatio = jawNoseDist / headHeight;

            // for perfectSync
            FaceBlendShape.JawOpen.Set(_blendShapeProxy, jawOpenRatio, true);

            FaceBlendShape.MouthClose.Set(_blendShapeProxy, mouthCenterNoseDist - mouthOpenDist, true);

            var smileLeft = upperLip.y - mouthCornerLeft.y;
            var smileRight = upperLip.y - mouthCornerRight.y;

            var mouthSmileLeft = 1 - FaceBlendShape.MouthSmileLeft.RemapBlendShape(smileLeft);
            var mouthSmileRight = 1 - FaceBlendShape.MouthSmileRight.RemapBlendShape(smileRight);
            
            FaceBlendShape.MouthSmileLeft.Set(_blendShapeProxy, mouthSmileLeft);
            FaceBlendShape.MouthSmileRight.Set(_blendShapeProxy, mouthSmileRight);
            
            FaceBlendShape.MouthDimpleLeft.Set(_blendShapeProxy, mouthSmileLeft * 0.5f);
            FaceBlendShape.MouthDimpleRight.Set(_blendShapeProxy, mouthSmileRight * 0.5f);

            var mouthFrownLeft = (mouthCornerLeft - data.PosList[CanonicalPoints.MouthFrownLeft].Point).y;
            var mouthFrownRight = (mouthCornerRight - data.PosList[CanonicalPoints.MouthFrownRight].Point).y;

            FaceBlendShape.MouthFrownLeft.Set(_blendShapeProxy, 1 - mouthFrownLeft);
            FaceBlendShape.MouthFrownRight.Set(_blendShapeProxy, 1 - mouthFrownRight);

            var mouthLeftStretchPoint = data.PosList[CanonicalPoints.MouthLeftStretch].Point;
            var mouthRightStretchPoint = data.PosList[CanonicalPoints.MouthRightStretch].Point;

            var mouthLeftStretch = mouthCornerLeft.x - mouthLeftStretchPoint.x;
            var mouthRightStretch = mouthCornerRight.x - mouthRightStretchPoint.x;
            var mouthCenterLeftStretch = mouthCenter.x - mouthLeftStretchPoint.x;
            var mouthCenterRightStretch = mouthCenter.x - mouthRightStretchPoint.x;

            var mouthLeft = FaceBlendShape.MouthLeft.RemapBlendShape(mouthCenterLeftStretch);
            var mouthRight = 1 - FaceBlendShape.MouthRight.RemapBlendShape(mouthCenterRightStretch);
            
            FaceBlendShape.MouthLeft.Set(_blendShapeProxy, mouthLeft);
            FaceBlendShape.MouthRight.Set(_blendShapeProxy, mouthRight);

            var stretchNormalLeft = -0.7f + 0.42f * mouthSmileLeft + 0.36f * mouthLeft;
            var stretchMaxLeft = -0.45f + 0.45f * mouthSmileLeft + 0.36f * mouthLeft;
            
            var stretchNormalRight = -0.7f + 0.42f * mouthSmileLeft + 0.36f * mouthLeft;
            var stretchMaxRight = -0.45f + 0.45f * mouthSmileLeft + 0.36f * mouthLeft;
            
            FaceBlendShape.MouthStretchLeft.Set(_blendShapeProxy, mouthLeftStretch.Remap(stretchNormalLeft, stretchMaxLeft));
            FaceBlendShape.MouthSmileRight.Set(_blendShapeProxy, mouthRightStretch.Remap(stretchNormalRight, stretchMaxRight));

            var uppestLip = data.PosList[0].Point;

            var jawRightLeft = noseTip.x - lowestChin.x;
            
            FaceBlendShape.JawLeft.Set(_blendShapeProxy, 1 - FaceBlendShape.JawLeft.RemapBlendShape(jawRightLeft));
            FaceBlendShape.JawRight.Set(_blendShapeProxy, jawRightLeft, true);

            var lowestLip = data.PosList[CanonicalPoints.LowestLip].Point;
            var underLip = data.PosList[CanonicalPoints.UnderLip].Point;

            var outerLipDist = Vector3.Distance(lowerLip, lowestLip);
            var upperLipDist = Vector3.Distance(upperLip, upperOuterLip);

            var mouthPucker = FaceBlendShape.MouthPucker.RemapBlendShape(mouthWidth);
            FaceBlendShape.MouthPucker.Set(_blendShapeProxy, 1 - mouthPucker);
            FaceBlendShape.MouthRollLower.Set(_blendShapeProxy, 1 - FaceBlendShape.MouthRollLower.RemapBlendShape(outerLipDist));
            FaceBlendShape.MouthRollUpper.Set(_blendShapeProxy, 1 - FaceBlendShape.MouthRollUpper.RemapBlendShape(upperLipDist));

            var upperLipNoseDist = noseTip.y - uppestLip.y;
            FaceBlendShape.MouthShrugUpper.Set(_blendShapeProxy, 1 - FaceBlendShape.MouthShrugUpper.RemapBlendShape(upperLipNoseDist));

            var overUpperLip = data.PosList[CanonicalPoints.OverUpperLip].Point;
            var mouthShrugLower = Vector3.Distance(lowestLip, overUpperLip);

            FaceBlendShape.MouthShrugLower.Set(_blendShapeProxy, 1 - FaceBlendShape.MouthShrugLower.RemapBlendShape(mouthShrugLower));

            var lowerDownLeft = Vector3.Distance(data.PosList[424].Point, data.PosList[319].Point) + mouthOpenDist * 0.5f;
            var lowerDownRight = Vector3.Distance(data.PosList[204].Point, data.PosList[89].Point) + mouthOpenDist * 0.5f;
            
            FaceBlendShape.MouthLowerDownLeft.Set(_blendShapeProxy, 1 - FaceBlendShape.MouthLowerDownLeft.RemapBlendShape(lowerDownLeft));
            FaceBlendShape.MouthLowerDownRight.Set(_blendShapeProxy, 1 - FaceBlendShape.MouthLowerDownRight.RemapBlendShape(lowerDownRight));

            FaceBlendShape.MouthFunnel.Set(_blendShapeProxy, mouthPucker < 0.5f ? 1 - FaceBlendShape.MouthFunnel.RemapBlendShape(mouthWidth) : 0);

            var leftUpperPressIndices = CanonicalPoints.LeftUpperPress;
            var leftLowerPressIndices = CanonicalPoints.LeftLowerPress;
            var rightUpperPressIndices = CanonicalPoints.RightUpperPress;
            var rightLowerPressIndices = CanonicalPoints.RightLowerPress;
            
            var leftUpperPress = Vector3.Distance(data.PosList[leftUpperPressIndices[0]].Point, data.PosList[leftUpperPressIndices[1]].Point);
            var leftLowerPress = Vector3.Distance(data.PosList[leftLowerPressIndices[0]].Point, data.PosList[leftLowerPressIndices[1]].Point);

            var mouthPressLeft = (leftUpperPress + leftLowerPress) * 0.5f;
            
            var rightUpperPress = Vector3.Distance(data.PosList[rightUpperPressIndices[0]].Point, data.PosList[rightUpperPressIndices[1]].Point);
            var rightLowerPress = Vector3.Distance(data.PosList[rightLowerPressIndices[0]].Point, data.PosList[rightLowerPressIndices[1]].Point);

            var mouthPressRight = (rightUpperPress + rightLowerPress) * 0.5f;
            
            FaceBlendShape.MouthPressLeft.Set(_blendShapeProxy, 1 - FaceBlendShape.MouthPressLeft.RemapBlendShape(mouthPressLeft));
            FaceBlendShape.MouthPressRight.Set(_blendShapeProxy, 1 - FaceBlendShape.MouthPressRight.RemapBlendShape(mouthPressRight));
        }

        private void EyeSolve(FaceData data)
        {
            if (_isPerfectSync)
            {
                // TODO
            }
            else
            {
                // TODO
            }
        }
    }
}
