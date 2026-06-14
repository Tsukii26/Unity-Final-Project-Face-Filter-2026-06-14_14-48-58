using UnityEngine;
using UnityEngine.XR.ARFoundation;
using FaceFilter.Core;

namespace FaceFilter.AR
{
    /// <summary>
    /// Chooses and builds the right "rig" for the current platform and exposes a single
    /// <see cref="IFilterController"/> to the UI:
    ///   • Android / iOS  → real AR Foundation face tracking (<see cref="ARRigBuilder"/>)
    ///   • Editor / desktop / WebGL → hardware-free <see cref="SimulatedFilterController"/>
    ///     with a procedural mannequin head, so the app is always demonstrable.
    /// Set <see cref="ForceSimulation"/> to true to preview simulation on a device too.
    /// </summary>
    public class AppRig
    {
        public static bool ForceSimulation = false;

        public GameObject Root { get; private set; }
        public IFilterController Filters { get; private set; }
        public ARCameraManager CameraManager { get; private set; } // null in simulation
        public ARFaceManager FaceManager { get; private set; }     // null in simulation
        public ARCameraSwitcher CameraSwitcher { get; private set; } // null in simulation
        public bool IsSimulation { get; private set; }

        public static bool ARAvailable
        {
            get
            {
                if (ForceSimulation) return false;
                return Application.platform == RuntimePlatform.Android
                    || Application.platform == RuntimePlatform.IPhonePlayer;
            }
        }

        public static AppRig Build()
        {
            var rig = new AppRig();
            if (ARAvailable) rig.BuildAR();
            else rig.BuildSimulation();
            return rig;
        }

        private void BuildAR()
        {
            IsSimulation = false;
            var arRig = ARRigBuilder.Build();
            Root = arRig.gameObject;
            Filters = arRig.FilterManager;
            CameraManager = arRig.CameraManager;
            FaceManager = arRig.FaceManager;
            CameraSwitcher = arRig.gameObject.AddComponent<ARCameraSwitcher>();
            CameraSwitcher.Initialize(arRig.CameraManager);
        }

        private void BuildSimulation()
        {
            IsSimulation = true;
            Root = new GameObject("Simulation Rig");

            // Camera in front of the face looking back at it.
            var camGo = new GameObject("Sim Camera");
            camGo.transform.SetParent(Root.transform, false);
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = AppTheme.Background;
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 10f;
            cam.fieldOfView = 45f;
            camGo.transform.position = new Vector3(0f, 0.02f, 0.55f);
            camGo.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            // Lighting.
            var keyLight = new GameObject("Key Light");
            keyLight.transform.SetParent(Root.transform, false);
            var l = keyLight.AddComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = 1.15f;
            l.transform.rotation = Quaternion.Euler(35f, 200f, 0f);

            var fill = new GameObject("Fill Light");
            fill.transform.SetParent(Root.transform, false);
            var l2 = fill.AddComponent<Light>();
            l2.type = LightType.Directional;
            l2.intensity = 0.4f;
            l2.color = AppTheme.Primary;
            l2.transform.rotation = Quaternion.Euler(10f, 140f, 0f);

            // Head anchor (face-space) + mannequin geometry.
            var headAnchor = new GameObject("Head").transform;
            headAnchor.SetParent(Root.transform, false);
            MannequinFactory.Build(headAnchor);

            var controller = Root.AddComponent<SimulatedFilterController>();
            controller.Initialize(headAnchor);
            Filters = controller;
        }
    }
}
