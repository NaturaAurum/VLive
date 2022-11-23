using UnityEngine;
namespace VLive.Runtime.Extensions
{
    public static class MathExtensions
    {
        public static float Remap(this float value, float min, float max)
        {
            return (Mathf.Clamp(value, min, max) - min) / (max - min);
        }
    }
}
