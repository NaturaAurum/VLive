using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VLive.Runtime.Models;
using VLive.Runtime.Utilities;
namespace VLive.Runtime.MediaPipe
{
    public class LandmarkDrawer : MonoBehaviour
    {
        [SerializeField]
        private Mesh mesh;
        [SerializeField]
        private Material mat;

        [SerializeField]
        private float poseScale;
        [SerializeField]
        private float faceScale;
        [SerializeField]
        private float handPointScale;
        [SerializeField]
        private float handScale;
        
        private ToggleModel PointToggle => StaticModels.Instance.PointToggle;
        
        private readonly List<OneEuroFilter<Vector3>> _oneEuroFilters = new();
        
        private const double KalmanTimeInterval = 0.45;
        private const double KalmanNoise = 0.4;
        private const float MinCutOffValue = 1.5f;
        private const float DCutOffValue = 1f;
        private const float Beta = 0.1f;
        private const float Frequency = 90f;

        private bool _isFilterInitialized = false;
        
        public void Draw(PoseData pose, FaceData face, HandsData hand)
        {
            if (!PointToggle.Toggle.Value)
            {
                return;
            }
            var index = 0;
            
            if (!_isFilterInitialized)
            {
                for (var i = 0; i < pose.NewPosList.Count; i++)
                {
                    _oneEuroFilters.Add(new OneEuroFilter<Vector3>(Frequency, MinCutOffValue, Beta, DCutOffValue));
                }
                _isFilterInitialized = true;
            }
            foreach (var euro in pose.NewPosList
                                     .Where(pointData => pointData.Visibility >= 0.75f)
                                     .Select(kalman => _oneEuroFilters[index].Filter(kalman)))
            {
                Draw(euro, poseScale);
                index++;
            }

            foreach (var pointData in face.PosList)
            {
                var offset = face.PosList[1].Point;
                var faceBase = pose.NewPosList[0].Point;
                var p = pointData.Point - offset;
                var newP = faceBase + p;
                Draw(newP, faceScale);
            }
            
            foreach (var pointData in hand.LeftPosList)
            {
                var offset = hand.LeftPosList[0].Point;
                var handBase = pose.NewPosList[15].Point;
                var p = pointData.Point - offset;
                p = Vector3.Scale(p, Vector3.one * handPointScale);
                var newP = handBase + p;
                Draw(newP, handScale);
            }
            
            foreach (var pointData in hand.RightPosList)
            {
                var offset = hand.RightPosList[0].Point;
                var handBase = pose.NewPosList[16].Point;
                var p = pointData.Point - offset;
                p = Vector3.Scale(p, Vector3.one * handPointScale);
                var newP = handBase + p;
                Draw(newP, handScale);
            }
        }

        private void Draw(Vector3 point, float scale)
        {
            Graphics.DrawMesh(mesh, Matrix4x4.TRS(point, Quaternion.identity, Vector3.one * scale), mat, gameObject.layer);
        }
    }
}
