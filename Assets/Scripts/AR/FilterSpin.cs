using UnityEngine;

namespace FaceFilter.AR
{
    /// <summary>Tiny helper that continuously rotates an object — used for animated filters.</summary>
    public class FilterSpin : MonoBehaviour
    {
        public Vector3 speed = new Vector3(0, 90f, 0);

        private void Update()
        {
            transform.localRotation *= Quaternion.Euler(speed * Time.deltaTime);
        }
    }
}
