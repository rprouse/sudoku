using System.Globalization;

namespace Sudoku.App.Converters;

internal static class ModeColors
{
    private static bool IsDark =>
        Application.Current?.RequestedTheme == AppTheme.Dark;

    public static Color SelectedBg() =>
        IsDark ? Color.FromArgb("#e0d8cf") : Color.FromArgb("#2d2925");

    public static Color UnselectedBg() =>
        IsDark ? Color.FromArgb("#2d2925") : Color.FromArgb("#e0d8cf");

    public static Color SelectedText() =>
        IsDark ? Color.FromArgb("#3d3632") : Color.FromArgb("#c8bfb3");

    public static Color UnselectedText() => Color.FromArgb("#7a7068");
}

public class BoolToModeColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? ModeColors.SelectedBg() : ModeColors.UnselectedBg();

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class BoolToModeTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? ModeColors.SelectedText() : ModeColors.UnselectedText();

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class InverseBoolToModeColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is false ? ModeColors.SelectedBg() : ModeColors.UnselectedBg();

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class InverseBoolToModeTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is false ? ModeColors.SelectedText() : ModeColors.UnselectedText();

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
