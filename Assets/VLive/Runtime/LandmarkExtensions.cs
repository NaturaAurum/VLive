using System.Collections.Generic;
using System.Linq;
using Mediapipe;
using Mediapipe.Unity;
using Mediapipe.Unity.CoordinateSystem;
using UnityEngine;
namespace VLive.Runtime
{
    public static class LandmarkExtensions
    {
        public static bool IsValid(this LandmarkList self)
        {
            return self is {Landmark: {Count: > 0}};
        }

        public static bool IsValid(this NormalizedLandmarkList self)
        {
            return self is {Landmark: {Count: > 0}};
        }
        
        public static PointData ApplyRotation(this PointData point, Quaternion rot)
        {
            var p = point.Point;
            p = rot * p;
            point.Point = p;
            return point;
        }

        public static List<PointData> ToVector3List(this IEnumerable<Landmark> landmarks, RectTransform rectTf, float scale)
        {
            return landmarks.Select(lm => new PointData
            {
                Point = lm.ToVector3(rectTf, scale),
                Visibility = lm.Visibility
            }).ToList();
        }

        public static List<PointData> ToVector3List(this IEnumerable<NormalizedLandmark> landmarks, RectTransform rectTf)
        {
            return landmarks.Select(lm => new PointData
            {
                Point = lm.ToVector3(rectTf),
                Visibility = lm.Visibility
            }).ToList();
        }

        public static PointData ToVector3(this NormalizedLandmark landmark)
        {
            return new PointData()
            {
                Point = new Vector3(landmark.X, landmark.Y, landmark.Z),
                Visibility = landmark.Visibility
            };
        }

        private static Vector3 ToVector3(this Landmark landmark, RectTransform rectTf, float scale)
        {
            var imageSource = ImageSourceProvider.ImageSource;
            var isMirrored = imageSource.isHorizontallyFlipped ^ imageSource.isFrontFacing;
            return rectTf.rect.GetPoint(landmark, Vector3.one * scale, imageSource.rotation.Reverse(), isMirrored);
        }

        private static Vector3 ToVector3(this NormalizedLandmark landmark, RectTransform rectTf)
        {
            var imageSource = ImageSourceProvider.ImageSource;
            var isMirrored = imageSource.isHorizontallyFlipped ^ imageSource.isFrontFacing;
            return rectTf.rect.GetPoint(landmark, imageSource.rotation.Reverse(), isMirrored);
        }
    }
}
