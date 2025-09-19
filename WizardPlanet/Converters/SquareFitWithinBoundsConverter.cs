using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace WizardPlanet.Converters
{
    public class SquareFitWithinBoundsConverter : IMultiValueConverter
    {
        private const double Sqrt2 = 1.4142135623730951;

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count < 2) return 0d;

            if (values[0] is double w && !double.IsNaN(w) && w > 0 &&
                values[1] is double h && !double.IsNaN(h) && h > 0)
            {
                // Size that guarantees the rotated square always fits in the window
                var fit = Math.Min(w, h) / Sqrt2;
                // Optional: small margin to avoid touching edges
                return Math.Max(0, fit - 2);
            }

            return 0d;
        }
    }
}