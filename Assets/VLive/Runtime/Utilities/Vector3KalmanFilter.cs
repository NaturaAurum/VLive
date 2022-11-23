using UnityEngine;
using VLive.Runtime.Extensions;
namespace VLive.Runtime.Utilities
{
    public class Vector3KalmanFilter
    {
        private readonly DiscreteKalmanFilter<ConstantVelocity3DModel, Vector3> _kalmanFilter;
        private int _effectiveCount;
        private const int StartCount = 50;
        
        public Vector3KalmanFilter(double timeInterval, double noise)
        {
            _kalmanFilter = new DiscreteKalmanFilter<ConstantVelocity3DModel, Vector3>(
                new ConstantVelocity3DModel()
                {
                    Position = Vector3.zero,
                    Velocity = Vector3.zero
                },
                ConstantVelocity3DModel.GetProcessNoise(noise, timeInterval),
                3,
                0,
                ConstantVelocity3DModel.ToArray,
                ConstantVelocity3DModel.FromArray,
                joint => new double[]
                {
                    joint.x,
                    joint.y,
                    joint.z
                }
            )
            {
                ProcessNoise = ConstantVelocity3DModel.GetProcessNoise(noise, timeInterval)
            };

            _kalmanFilter.MeasurementNoise = MatrixFunctions.Diagonal(_kalmanFilter.MeasurementVectorDimension, 1.0);
            _kalmanFilter.MeasurementMatrix = ConstantVelocity3DModel.GetPositionMeasurementMatrix();
            _kalmanFilter.TransitionMatrix = ConstantVelocity3DModel.GetTransitionMatrix(timeInterval);
            _kalmanFilter.Predict();
        }

        public void UpdateFilterParameter(double timeInterval, double noise)
        {
            _kalmanFilter.ProcessNoise = ConstantVelocity3DModel.GetProcessNoise(noise, timeInterval);
            _kalmanFilter.TransitionMatrix = ConstantVelocity3DModel.GetTransitionMatrix(timeInterval);
            _effectiveCount = 0;
        }

        private void Correct(Vector3 v) => _kalmanFilter.Correct(v);
        private void Predict() => _kalmanFilter.Predict();
        private Vector3 GetPosition() => _kalmanFilter.State.Position;

        public Vector3 CorrectAndPredict(Vector3 v)
        {
            Correct(v);
            Predict();
            if (_effectiveCount >= 50)
            {
                return GetPosition();
            }

            ++_effectiveCount;
            return v;
        }
    }
}
