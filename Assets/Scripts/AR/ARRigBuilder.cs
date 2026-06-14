using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.SpatialTracking;
using Unity.XR.CoreUtils;

namespace FaceFilter.AR
{
    /// <summary>
    /// Builds a complete AR Foundation face-tracking rig from code: AR Session,
    /// XR Origin, AR camera (with background + pose driver), the AR Face Manager and
    /// our <see cref="FaceFilterManager"/>. Building it in code keeps the scene files
    /// trivial and guarantees the rig is always wired correctly.
    /// </summary>
    public class ARRigBuilder : MonoBehaviour
    {
        public ARSession Session { get; private set; }
        public XROrigin Origin { get; private set; }
        public Camera ARCamera { get; private set; }
        public ARCameraManager CameraManager { get; private set; }
        public ARFaceManager FaceManager { get; private set; }
        public FaceFilterManager FilterManager { get; private set; }

        public static ARRigBuilder Build()
        {
            var builderGo = new GameObject("AR Rig");
            var builder = builderGo.AddComponent<ARRigBuilder>();
            builder.Construct();
            return builder;
        }

        private void Construct()
        {
            // 1) AR Session.
            var sessionGo = new GameObject("AR Session");
            sessionGo.transform.SetParent(transform, false);
            Session = sessionGo.AddComponent<ARSession>();
            sessionGo.AddComponent<ARInputManager>();

            // 2) XR Origin.
            var originGo = new GameObject("XR Origin");
            originGo.transform.SetParent(transform, false);
            Origin = originGo.AddComponent<XROrigin>();

            // Camera offset object (required by XROrigin).
            var offsetGo = new GameObject("Camera Offset");
            offsetGo.transform.SetParent(originGo.transform, false);
            Origin.CameraFloorOffsetObject = offsetGo;

            // 3) AR Camera.
            var camGo = new GameObject("AR Camera");
            camGo.transform.SetParent(offsetGo.transform, false);
            camGo.tag = "MainCamera";
            ARCamera = camGo.AddComponent<Camera>();
            ARCamera.clearFlags = CameraClearFlags.SolidColor;
            ARCamera.backgroundColor = Color.black;
            ARCamera.nearClipPlane = 0.05f;
            ARCamera.farClipPlane = 20f;

            CameraManager = camGo.AddComponent<ARCameraManager>();
            camGo.AddComponent<ARCameraBackground>();

            var poseDriver = camGo.AddComponent<TrackedPoseDriver>();
            poseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRDevice, TrackedPoseDriver.TrackedPose.ColorCamera);
            poseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
            poseDriver.updateType = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;

            Origin.Camera = ARCamera;
            Origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Device;

            // 4) Face tracking + filters.
            FaceManager = originGo.AddComponent<ARFaceManager>();
            FilterManager = originGo.AddComponent<FaceFilterManager>();

            // A soft light so the procedural 3D filters are visible.
            var lightGo = new GameObject("Filter Light");
            lightGo.transform.SetParent(transform, false);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
        }
    }
}
