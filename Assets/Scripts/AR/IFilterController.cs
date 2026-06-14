using System;

namespace FaceFilter.AR
{
    /// <summary>
    /// Common control surface for the UI, implemented by both the real AR face-tracking
    /// controller (<see cref="FaceFilterManager"/>) and the hardware-free
    /// <see cref="SimulatedFilterController"/>. Lets the UI stay identical in both modes.
    /// </summary>
    public interface IFilterController
    {
        event Action<FilterEntry> FilterChanged;

        bool FiltersEnabled { get; }
        int CurrentIndex { get; }
        FilterEntry Current { get; }

        void ToggleFilters();
        void SetFiltersEnabled(bool on);
        void NextFilter();
        void PreviousFilter();
        void SetFilter(int index);
        void ResetFilters();
    }
}
