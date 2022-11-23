using System.Collections.Generic;
using Mediapipe;
using UnityEngine;
namespace VLive.Runtime.MediaPipe
{
    public struct PointData
    {
        public Vector3 Point;
        public float Visibility;
    }
    
    public class PoseData
    {
        public Vector3 HipPoint 
        {
            get
            {
                if (NewPosList.Count < 24)
                {
                    return Vector3.zero;
                }
                var upperCenter = Vector3.Lerp(NewPosList[11].Point, NewPosList[12].Point, 0.5f);
                var lowerCenter = Vector3.Lerp(NewPosList[23].Point, NewPosList[24].Point, 0.5f);

                var center = Vector3.Lerp(upperCenter, lowerCenter, 0.7f);
                return center;
            }
        }
        public List<PointData> NewPosList = new();
    }
    
    public class HandsData
    {
        public List<PointData> LeftPosList = new();
        public List<PointData> RightPosList = new();
    }
    
    // for blendshape
    // 고개 회전은 pose 데이터쪽으로
    public class FaceData
    {
        public List<PointData> PosList = new();
        public NormalizedLandmarkList Origin;
    }
}
