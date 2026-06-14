using System.Collections.Generic;

namespace FaceFilter.AR
{
    public struct FilterEntry
    {
        public string DisplayName;
        public string Glyph; // shown in the UI chip
        public FilterType Type;

        public FilterEntry(string name, string glyph, FilterType type)
        {
            DisplayName = name;
            Glyph = glyph;
            Type = type;
        }
    }

    /// <summary>The ordered list of filters the user can cycle through.</summary>
    public static class FilterCatalog
    {
        public static readonly List<FilterEntry> Entries = new List<FilterEntry>
        {
            new FilterEntry("None", "\u2715", FilterType.None),
            new FilterEntry("Glasses", "\U0001F453", FilterType.Glasses),
            new FilterEntry("Sunglasses", "\U0001F576", FilterType.Sunglasses),
            new FilterEntry("Mask", "\U0001F3AD", FilterType.Mask),
            new FilterEntry("Party Hat", "\U0001F389", FilterType.PartyHat),
            new FilterEntry("Mustache", "\U0001F468", FilterType.Mustache),
            new FilterEntry("Hearts", "\u2764", FilterType.HeartStickers),
            new FilterEntry("Halo", "\U0001F607", FilterType.Halo)
        };

        public static FilterEntry Get(int index)
        {
            if (Entries.Count == 0) return new FilterEntry("None", "\u2715", FilterType.None);
            index = ((index % Entries.Count) + Entries.Count) % Entries.Count;
            return Entries[index];
        }
    }
}
