using System.Collections.Generic;
using UnityEngine;

namespace FaceFilter.AR
{
    public enum FilterType
    {
        None,
        Glasses,
        Sunglasses,
        Mask,
        PartyHat,
        Mustache,
        HeartStickers,
        Halo
    }

    /// <summary>
    /// Builds 3D face-filter content procedurally from Unity primitives so the project
    /// works out-of-the-box without any imported art. Each filter returns a root
    /// GameObject whose local space is the face anchor (origin ~ center of the head,
    /// +Z pointing out of the face, +Y up).
    /// </summary>
    public static class FilterContentFactory
    {
        // Approximate face-space offsets (metres). The face anchor sits near the nose.
        private static readonly Vector3 EyeLevel = new Vector3(0f, 0.02f, 0.06f);
        private const float EyeSeparation = 0.064f;

        public static GameObject Build(FilterType type)
        {
            var root = new GameObject("Filter_" + type);
            switch (type)
            {
                case FilterType.Glasses: BuildGlasses(root.transform, false); break;
                case FilterType.Sunglasses: BuildGlasses(root.transform, true); break;
                case FilterType.Mask: BuildMask(root.transform); break;
                case FilterType.PartyHat: BuildPartyHat(root.transform); break;
                case FilterType.Mustache: BuildMustache(root.transform); break;
                case FilterType.HeartStickers: BuildHeartStickers(root.transform); break;
                case FilterType.Halo: BuildHalo(root.transform); break;
            }
            return root;
        }

        private static void BuildGlasses(Transform parent, bool tinted)
        {
            var frame = Mat(new Color(0.05f, 0.05f, 0.07f), 0.2f, 0.7f);
            var lensColor = tinted ? new Color(0.02f, 0.02f, 0.02f, 0.85f) : new Color(0.6f, 0.85f, 1f, 0.35f);
            var lens = TransparentMat(lensColor);

            CreateLens(parent, new Vector3(-EyeSeparation, EyeLevel.y, EyeLevel.z), lens, frame);
            CreateLens(parent, new Vector3(EyeSeparation, EyeLevel.y, EyeLevel.z), lens, frame);

            // Bridge.
            var bridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Strip(bridge);
            bridge.transform.SetParent(parent, false);
            bridge.transform.localPosition = new Vector3(0, EyeLevel.y + 0.005f, EyeLevel.z);
            bridge.transform.localScale = new Vector3(0.03f, 0.006f, 0.006f);
            Paint(bridge, frame);

            // Temple arms.
            CreateArm(parent, -1, frame);
            CreateArm(parent, 1, frame);
        }

        private static void CreateLens(Transform parent, Vector3 pos, Material lensMat, Material frameMat)
        {
            var lens = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Strip(lens);
            lens.transform.SetParent(parent, false);
            lens.transform.localPosition = pos;
            lens.transform.localRotation = Quaternion.Euler(90, 0, 0);
            lens.transform.localScale = new Vector3(0.052f, 0.004f, 0.04f);
            Paint(lens, lensMat);

            var rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Strip(rim);
            rim.transform.SetParent(parent, false);
            rim.transform.localPosition = pos + new Vector3(0, 0, -0.002f);
            rim.transform.localRotation = Quaternion.Euler(90, 0, 0);
            rim.transform.localScale = new Vector3(0.06f, 0.003f, 0.046f);
            Paint(rim, frameMat);
        }

        private static void CreateArm(Transform parent, int side, Material mat)
        {
            var arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Strip(arm);
            arm.transform.SetParent(parent, false);
            arm.transform.localPosition = new Vector3(side * 0.075f, EyeLevel.y + 0.005f, EyeLevel.z - 0.04f);
            arm.transform.localScale = new Vector3(0.01f, 0.008f, 0.09f);
            Paint(arm, mat);
        }

        private static void BuildMask(Transform parent)
        {
            var mat = Mat(new Color(0.1f, 0.15f, 0.35f), 0.1f, 0.4f);
            var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Strip(body);
            body.transform.SetParent(parent, false);
            body.transform.localPosition = new Vector3(0, -0.01f, 0.045f);
            body.transform.localScale = new Vector3(0.16f, 0.12f, 0.09f);
            Paint(body, mat);

            // Eye holes (dark).
            var holeMat = Mat(Color.black, 0f, 0f);
            foreach (var s in new[] { -1, 1 })
            {
                var hole = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Strip(hole);
                hole.transform.SetParent(parent, false);
                hole.transform.localPosition = new Vector3(s * EyeSeparation, 0.02f, 0.085f);
                hole.transform.localScale = new Vector3(0.045f, 0.03f, 0.02f);
                Paint(hole, holeMat);
            }
        }

        private static void BuildPartyHat(Transform parent)
        {
            var cone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Strip(cone);
            cone.transform.SetParent(parent, false);
            cone.transform.localPosition = new Vector3(0, 0.14f, 0.02f);
            cone.transform.localScale = new Vector3(0.11f, 0.001f, 0.11f);
            Paint(cone, Mat(new Color(0.95f, 0.3f, 0.5f), 0.1f, 0.3f));

            var tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Strip(tip);
            tip.transform.SetParent(cone.transform, false);
            tip.transform.localPosition = new Vector3(0, 60f, 0);
            tip.transform.localScale = new Vector3(0.25f, 30f, 0.25f);
            Paint(tip, Mat(new Color(1f, 0.85f, 0.2f), 0.4f, 0.6f));
            // Stretch cone into an actual cone-ish shape using non-uniform scale.
            cone.transform.localScale = new Vector3(0.11f, 0.09f, 0.11f);
        }

        private static void BuildMustache(Transform parent)
        {
            var mat = Mat(new Color(0.12f, 0.08f, 0.05f), 0.1f, 0.5f);
            foreach (var s in new[] { -1, 1 })
            {
                var half = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                Strip(half);
                half.transform.SetParent(parent, false);
                half.transform.localPosition = new Vector3(s * 0.02f, -0.03f, 0.09f);
                half.transform.localRotation = Quaternion.Euler(90, 0, s * 25f);
                half.transform.localScale = new Vector3(0.018f, 0.03f, 0.018f);
                Paint(half, mat);
            }
        }

        private static void BuildHeartStickers(Transform parent)
        {
            var mat = TransparentMat(new Color(1f, 0.25f, 0.45f, 0.95f));
            var rnd = new System.Random(7);
            for (int i = 0; i < 6; i++)
            {
                var heart = new GameObject("Heart" + i);
                heart.transform.SetParent(parent, false);
                float angle = i * 60f * Mathf.Deg2Rad;
                heart.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.11f, 0.06f + Mathf.Sin(angle) * 0.08f, 0.05f);
                heart.transform.localScale = Vector3.one * (0.6f + (float)rnd.NextDouble() * 0.6f);
                BuildHeartShape(heart.transform, mat);
                var spin = heart.AddComponent<FilterSpin>();
                spin.speed = new Vector3(0, 90f + i * 10f, 0);
            }
        }

        private static void BuildHeartShape(Transform parent, Material mat)
        {
            foreach (var s in new[] { -1, 1 })
            {
                var lobe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Strip(lobe);
                lobe.transform.SetParent(parent, false);
                lobe.transform.localPosition = new Vector3(s * 0.012f, 0.01f, 0);
                lobe.transform.localScale = Vector3.one * 0.025f;
                Paint(lobe, mat);
            }
            var point = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Strip(point);
            point.transform.SetParent(parent, false);
            point.transform.localPosition = new Vector3(0, -0.012f, 0);
            point.transform.localRotation = Quaternion.Euler(0, 0, 45);
            point.transform.localScale = new Vector3(0.026f, 0.026f, 0.02f);
            Paint(point, mat);
        }

        private static void BuildHalo(Transform parent)
        {
            var ring = new GameObject("Halo");
            ring.transform.SetParent(parent, false);
            ring.transform.localPosition = new Vector3(0, 0.16f, 0.02f);
            ring.transform.localRotation = Quaternion.Euler(80, 0, 0);
            var mat = EmissiveMat(new Color(1f, 0.9f, 0.35f));
            int segments = 24;
            float radius = 0.07f;
            for (int i = 0; i < segments; i++)
            {
                float a = (i / (float)segments) * Mathf.PI * 2f;
                var bead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Strip(bead);
                bead.transform.SetParent(ring.transform, false);
                bead.transform.localPosition = new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0);
                bead.transform.localScale = Vector3.one * 0.014f;
                Paint(bead, mat);
            }
            var spin = ring.AddComponent<FilterSpin>();
            spin.speed = new Vector3(0, 0, 60f);
        }

        // --- material/primitive helpers ---

        private static void Strip(GameObject go)
        {
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
        }

        private static void Paint(GameObject go, Material mat)
        {
            var r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = mat;
        }

        private static Shader UrpOrStandard()
        {
            var s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Standard");
            return s;
        }

        private static Material Mat(Color color, float metallic, float smoothness)
        {
            var m = new Material(UrpOrStandard());
            m.color = color;
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smoothness);
            return m;
        }

        private static Material TransparentMat(Color color)
        {
            var m = Mat(color, 0.1f, 0.8f);
            m.SetFloat("_Mode", 3);
            if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1);
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.DisableKeyword("_ALPHATEST_ON");
            m.EnableKeyword("_ALPHABLEND_ON");
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            return m;
        }

        private static Material EmissiveMat(Color color)
        {
            var m = Mat(color, 0.2f, 0.9f);
            if (m.HasProperty("_EmissionColor"))
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", color * 2f);
            }
            return m;
        }
    }
}
