using UnityEngine;

namespace FaceFilter.AR
{
    /// <summary>
    /// Builds a simple procedural mannequin head (skin + eyes + nose + neck) used by the
    /// simulation rig so filters have a face to sit on. Geometry is placed in the same
    /// face-space the filters expect (origin near the nose, +Z out of the face).
    /// </summary>
    public static class MannequinFactory
    {
        public static void Build(Transform parent)
        {
            var skin = Mat(new Color(0.86f, 0.69f, 0.58f), 0.45f);

            var head = Prim(PrimitiveType.Sphere, parent, "Head");
            head.localPosition = new Vector3(0f, 0.0f, 0.0f);
            head.localScale = new Vector3(0.17f, 0.21f, 0.19f);
            Paint(head, skin);

            var neck = Prim(PrimitiveType.Cylinder, parent, "Neck");
            neck.localPosition = new Vector3(0f, -0.16f, -0.01f);
            neck.localScale = new Vector3(0.07f, 0.08f, 0.07f);
            Paint(neck, skin);

            // Eyes (whites + pupils).
            var white = Mat(Color.white, 0.2f);
            var pupil = Mat(new Color(0.06f, 0.06f, 0.08f), 0.1f);
            foreach (var s in new[] { -1, 1 })
            {
                var eye = Prim(PrimitiveType.Sphere, parent, "Eye");
                eye.localPosition = new Vector3(s * 0.055f, 0.03f, 0.082f);
                eye.localScale = new Vector3(0.03f, 0.022f, 0.012f);
                Paint(eye, white);

                var p = Prim(PrimitiveType.Sphere, parent, "Pupil");
                p.localPosition = new Vector3(s * 0.055f, 0.03f, 0.09f);
                p.localScale = new Vector3(0.012f, 0.012f, 0.008f);
                Paint(p, pupil);
            }

            var nose = Prim(PrimitiveType.Sphere, parent, "Nose");
            nose.localPosition = new Vector3(0f, -0.01f, 0.095f);
            nose.localScale = new Vector3(0.028f, 0.04f, 0.03f);
            Paint(nose, skin);

            var mouth = Prim(PrimitiveType.Cube, parent, "Mouth");
            mouth.localPosition = new Vector3(0f, -0.07f, 0.082f);
            mouth.localScale = new Vector3(0.05f, 0.008f, 0.01f);
            Paint(mouth, Mat(new Color(0.6f, 0.3f, 0.32f), 0.2f));
        }

        private static Transform Prim(PrimitiveType type, Transform parent, string name)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static void Paint(Transform t, Material m)
        {
            var r = t.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = m;
        }

        private static Material Mat(Color color, float smoothness)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            var m = new Material(shader);
            m.color = color;
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smoothness);
            return m;
        }
    }
}
