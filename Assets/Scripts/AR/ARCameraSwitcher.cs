using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace FaceFilter.AR
{
    /// <summary>
    /// Switches between the front (user-facing) and back (world-facing) cameras at
    /// runtime. Face filters require the user-facing camera; the back camera is offered
    /// where the device supports it (e.g. capturing the environment).
    /// </summary>
    public class ARCameraSwitcher : MonoBehaviour
    {
        private ARCameraManager _cameraManager;

        public CameraFacingDirection CurrentDirection
        {
            get { return _cameraManager != null ? _cameraManager.currentFacingDirection : CameraFacingDirection.User; }
        }

        public bool IsFrontFacing
        {
            get { return CurrentDirection != CameraFacingDirection.World; }
        }

        public void Initialize(ARCameraManager cameraManager)
        {
            _cameraManager = cameraManager;
            if (_cameraManager != null)
                _cameraManager.requestedFacingDirection = CameraFacingDirection.User;
        }

        public void Toggle()
        {
            if (_cameraManager == null) return;
            _cameraManager.requestedFacingDirection =
                IsFrontFacing ? CameraFacingDirection.World : CameraFacingDirection.User;
        }
    }
}
