using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
namespace VLive.Runtime
{
    public class CameraController : MonoBehaviour
    {
        private const float MouseXSpeed = 30f;
        private const float MouseYSpeed = 30f;
        private const float ZoomSpeed = 10f;
        
        [SerializeField]
        private Transform cameraTransform;

        private const string MouseX = "Mouse X";
        private const string MouseY = "Mouse Y";
        private const string Scroll = "Mouse ScrollWheel";

        private float _mouseXRotateVelocity;
        private float _mouseYRotateVelocity;

        private const float Damp = 0.15f; 

        private void Awake()
        {
            UniTaskAsyncEnumerable.EveryUpdate().Subscribe(EveryUpdate).AddTo(this.GetCancellationTokenOnDestroy());
        }
        private void EveryUpdate(AsyncUnit _)
        {
            var leftAltPress = Input.GetKey(KeyCode.LeftAlt);
            var leftControlPress = Input.GetKey(KeyCode.LeftControl);
            var leftMousePress = Input.GetMouseButton(0);
            var rightMousePress = Input.GetMouseButton(1);
            var rotationMode =  leftAltPress && leftMousePress;
            var moveMode = leftControlPress && rightMousePress;
            var zoomMode = leftAltPress && rightMousePress;

            if (rotationMode)
            {
                Rotate();
            }

            if (zoomMode)
            {
                Zoom();
            }

            if (moveMode)
            {
                Move();
            }
        }

        private void Rotate()
        {
            var mouseX = Input.GetAxis(MouseX);
            var mouseY = Input.GetAxis(MouseY);

            var tf = transform;
            var eulerAngles = tf.eulerAngles;
            // var nextX = eulerAngles.x - mouseY * MouseXSpeed;
            // var nextY = eulerAngles.y + mouseX * MouseYSpeed;
            // var dampX = Mathf.SmoothDampAngle(eulerAngles.x, nextX, ref _mouseXRotateVelocity, Damp);
            // var dampY = Mathf.SmoothDampAngle(eulerAngles.y, nextY, ref _mouseXRotateVelocity, Damp);
            var position = tf.position;
            cameraTransform.RotateAround(position, Vector3.right, -mouseY * MouseXSpeed);
            cameraTransform.RotateAround(position, Vector3.up, mouseX * MouseYSpeed);
            // var xRotation = Quaternion.AngleAxis(dampX, tf.right);
            // var yRotation = Quaternion.AngleAxis(dampY, tf.up);
            // tf.rotation = yRotation * xRotation;
        }

        private void Zoom()
        {
            
        }

        private void Move()
        {
            
        }
    }
}
