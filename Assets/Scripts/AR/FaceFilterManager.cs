using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using FaceFilter.Core;

namespace FaceFilter.AR
{
    /// <summary>
    /// Owns the face-filter logic: listens to <see cref="ARFaceManager"/> for tracked
    /// faces, spawns the currently selected filter onto each face anchor so it follows
    /// head movement, and exposes runtime controls (toggle, next/previous, select).
    /// </summary>
    [RequireComponent(typeof(ARFaceManager))]
    public class FaceFilterManager : MonoBehaviour, IFilterController
    {
        public event Action<FilterEntry> FilterChanged;

        private ARFaceManager _faceManager;
        private readonly Dictionary<TrackableId, GameObject> _spawned = new Dictionary<TrackableId, GameObject>();

        private int _currentIndex;
        private bool _enabled = true;

        public bool FiltersEnabled { get { return _enabled; } }
        public int CurrentIndex { get { return _currentIndex; } }
        public FilterEntry Current { get { return FilterCatalog.Get(_currentIndex); } }

        private void Awake()
        {
            _faceManager = GetComponent<ARFaceManager>();
            _currentIndex = Mathf.Clamp(AppSettings.LastFilterIndex, 0, FilterCatalog.Entries.Count - 1);
        }

        private void OnEnable()
        {
            if (_faceManager != null) _faceManager.facesChanged += OnFacesChanged;
        }

        private void OnDisable()
        {
            if (_faceManager != null) _faceManager.facesChanged -= OnFacesChanged;
        }

        private void OnFacesChanged(ARFacesChangedEventArgs args)
        {
            foreach (var face in args.added) AttachFilter(face);
            foreach (var face in args.removed)
            {
                if (_spawned.TryGetValue(face.trackableId, out var go))
                {
                    if (go != null) Destroy(go);
                    _spawned.Remove(face.trackableId);
                }
            }
        }

        private void AttachFilter(ARFace face)
        {
            ClearFace(face.trackableId);
            if (!_enabled) return;

            var entry = FilterCatalog.Get(_currentIndex);
            if (entry.Type == FilterType.None) return;

            var content = FilterContentFactory.Build(entry.Type);
            content.transform.SetParent(face.transform, false);
            content.transform.localPosition = Vector3.zero;
            content.transform.localRotation = Quaternion.identity;
            _spawned[face.trackableId] = content;
        }

        private void ClearFace(TrackableId id)
        {
            if (_spawned.TryGetValue(id, out var go))
            {
                if (go != null) Destroy(go);
                _spawned.Remove(id);
            }
        }

        private void RebuildAll()
        {
            var ids = new List<TrackableId>(_spawned.Keys);
            foreach (var id in ids) ClearFace(id);

            if (_faceManager == null) return;
            foreach (var face in _faceManager.trackables) AttachFilter(face);
        }

        // --- Public runtime controls (wired to UI buttons) ---

        public void ToggleFilters()
        {
            SetFiltersEnabled(!_enabled);
        }

        public void SetFiltersEnabled(bool on)
        {
            _enabled = on;
            RebuildAll();
        }

        public void NextFilter()
        {
            SetFilter(_currentIndex + 1);
        }

        public void PreviousFilter()
        {
            SetFilter(_currentIndex - 1);
        }

        public void SetFilter(int index)
        {
            int count = FilterCatalog.Entries.Count;
            _currentIndex = ((index % count) + count) % count;
            AppSettings.LastFilterIndex = _currentIndex;
            _enabled = true;
            RebuildAll();
            if (FilterChanged != null) FilterChanged(Current);
        }

        public void ResetFilters()
        {
            SetFilter(0);
            _enabled = true;
        }
    }
}
