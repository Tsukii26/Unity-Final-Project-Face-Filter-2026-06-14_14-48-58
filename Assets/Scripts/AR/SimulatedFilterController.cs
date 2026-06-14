using System;
using UnityEngine;
using FaceFilter.Core;

namespace FaceFilter.AR
{
    /// <summary>
    /// Hardware-free stand-in for <see cref="FaceFilterManager"/>. Instead of tracking a
    /// real face, it places the selected filter on a procedural mannequin head that
    /// gently turns side to side — so the full app (UI, filters, capture, demo) is
    /// demonstrable in the Unity Editor and on desktop/WebGL with no ARKit/ARCore device.
    /// </summary>
    public class SimulatedFilterController : MonoBehaviour, IFilterController
    {
        public event Action<FilterEntry> FilterChanged;

        private Transform _headAnchor;     // matches AR face-space: origin ~ nose, +Z out of face
        private GameObject _currentFilter;
        private int _currentIndex;
        private bool _enabled = true;
        private float _swayAmount = 18f;

        public bool FiltersEnabled { get { return _enabled; } }
        public int CurrentIndex { get { return _currentIndex; } }
        public FilterEntry Current { get { return FilterCatalog.Get(_currentIndex); } }
        public Transform HeadAnchor { get { return _headAnchor; } }

        public void Initialize(Transform headAnchor)
        {
            _headAnchor = headAnchor;
            _currentIndex = Mathf.Clamp(AppSettings.LastFilterIndex, 0, FilterCatalog.Entries.Count - 1);
            Rebuild();
        }

        private void Update()
        {
            if (_headAnchor == null) return;
            // Gentle head sway so filters visibly track the face.
            float yaw = Mathf.Sin(Time.time * 0.6f) * _swayAmount;
            float pitch = Mathf.Sin(Time.time * 0.4f) * (_swayAmount * 0.3f);
            _headAnchor.localRotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private void Rebuild()
        {
            if (_currentFilter != null) Destroy(_currentFilter);
            _currentFilter = null;
            if (!_enabled || _headAnchor == null) return;

            var entry = FilterCatalog.Get(_currentIndex);
            if (entry.Type == FilterType.None) return;

            _currentFilter = FilterContentFactory.Build(entry.Type);
            _currentFilter.transform.SetParent(_headAnchor, false);
            _currentFilter.transform.localPosition = Vector3.zero;
            _currentFilter.transform.localRotation = Quaternion.identity;
        }

        public void ToggleFilters() { SetFiltersEnabled(!_enabled); }

        public void SetFiltersEnabled(bool on)
        {
            _enabled = on;
            Rebuild();
        }

        public void NextFilter() { SetFilter(_currentIndex + 1); }
        public void PreviousFilter() { SetFilter(_currentIndex - 1); }

        public void SetFilter(int index)
        {
            int count = FilterCatalog.Entries.Count;
            _currentIndex = ((index % count) + count) % count;
            AppSettings.LastFilterIndex = _currentIndex;
            _enabled = true;
            Rebuild();
            if (FilterChanged != null) FilterChanged(Current);
        }

        public void ResetFilters()
        {
            SetFilter(0);
            _enabled = true;
        }
    }
}
