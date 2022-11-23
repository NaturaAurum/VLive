using UnityEngine;
namespace VLive.Runtime.MediaPipe
{
    public enum JointIndex
    {
        RightUpperArm = 14,
        RightLowerArm = 16,
        RightHand = 18,
        RightThumb2 = 40,
        RightMid1 = 45,
        LeftUpperArm = 13,
        LeftLowerArm = 15,
        LeftHand = 17,
        LeftThumb2 = 25,
        LeftMid1 = 28,
        LeftEye = 21,
        RightEye = 22,
        RightThigh = 2,
        RightShin = 4,
        RightFoot = 6,
        RightToe = 20,
        LeftThigh = 1,
        LeftShin = 3,
        LeftFoot = 5,
        LeftToe = 19,
        Hip = 0,
        Head = 10,
        Neck = 9,
        Spine = 7,
        RightShoulder = 12,
        LeftShoulder = 11,
        Chest = 8,
        LeftEar = 100,
        RightEar = 101,
        Nose = 102,
        AbdomenUpper = 103,
        Crotch = 104,
    }

    public static class JointIndexExtensions
    {
        public static int Int(this JointIndex index)
        {
            return (int)index;
        }

        public static HumanBodyBones ToHumanBodyBones(this JointIndex index)
        {
            switch (index)
            {
                case JointIndex.AbdomenUpper:
                    return HumanBodyBones.Spine;
                case JointIndex.Nose:
                case JointIndex.LeftEar:
                case JointIndex.RightEar:
                case JointIndex.Crotch:
                    return HumanBodyBones.LastBone;
                default:
                    return (HumanBodyBones)index.Int();
            }
        }
    }

    public class JointPoint
    {
        public bool Enabled;
        public Vector3 Pos3D;
        public Vector3 Now3D;

        public Transform Transform;
        public Quaternion InitRotation;
        public Quaternion InitLocalRotation;
        public Quaternion Inverse;
        public Quaternion InverseRotation;

        public JointPoint Child;
        public JointPoint Parent;
        
        private const float VisibleThreshold = 0.75f;

        public void SetNow3D(PointData pointData)
        {
            Enabled = pointData.Visibility >= VisibleThreshold;
            if (Enabled)
            {
                Now3D = pointData.Point;
            }
        }
    }
}
