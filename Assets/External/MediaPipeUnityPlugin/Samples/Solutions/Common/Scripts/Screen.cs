// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Mediapipe.Unity
{
    public class Screen : MonoBehaviour
    {
        [SerializeField] private RawImage screen;
        [SerializeField] private float ratio = 1;
        private ImageSource _imageSource;

        private static readonly UnityEngine.Rect DefaultRect = new(0, 0, 1, 1);

        public Texture Texture
        {
            private get => screen.texture;
            set => screen.texture = value;
        }

        public UnityEngine.Rect UVRect
        {
            set => screen.uvRect = value;
        }

        public void Initialize(ImageSource imageSource)
        {
            _imageSource = imageSource;

            Resize(_imageSource.textureWidth, _imageSource.textureHeight);
            Rotate(_imageSource.rotation.Reverse());
            ResetUvRect(RunningMode.Async);
            Texture = imageSource.GetCurrentTexture();
        }

        public void Resize(int width, int height)
        {
            var size = screen.rectTransform.sizeDelta;
            size.x = width;
            size.y = height;
            size *= ratio;
            screen.rectTransform.sizeDelta = size;
        }

        public void Rotate(RotationAngle rotationAngle)
        {
            screen.rectTransform.localEulerAngles = rotationAngle.GetEulerAngles();
        }

        public void ReadSync(TextureFrame textureFrame)
        {
            return;
            // if (!(texture is Texture2D))
            // {
            //     texture = new Texture2D(_imageSource.textureWidth, _imageSource.textureHeight, TextureFormat.RGBA32, false);
            //     ResetUvRect(RunningMode.Sync);
            // }
            // textureFrame.CopyTexture(texture);
        }

        private void ResetUvRect(RunningMode runningMode)
        {
            var rect = DefaultRect;

            if (_imageSource.isVerticallyFlipped && runningMode == RunningMode.Async)
            {
                // In Async mode, we don't need to flip the screen vertically since the image will be copied on CPU.
                FlipVertically(ref rect);
            }

            if (_imageSource.isFrontFacing)
            {
                // Flip the image (not the screen) horizontally.
                // It should be taken into account that the image will be rotated later.
                var rotation = _imageSource.rotation;

                if (rotation is RotationAngle.Rotation0 or RotationAngle.Rotation180)
                {
                    FlipHorizontally(ref rect);
                }
                else
                {
                    FlipVertically(ref rect);
                }
            }

            UVRect = rect;
        }

        private static void FlipHorizontally(ref UnityEngine.Rect rect)
        {
            rect.x = 1 - rect.x;
            rect.width = -rect.width;
        }

        private static void FlipVertically(ref UnityEngine.Rect rect)
        {
            rect.y = 1 - rect.y;
            rect.height = -rect.height;
        }
    }
}
