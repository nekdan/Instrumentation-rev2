using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Instrumentation.Dialogs;

namespace Instrumentation.Views.Converters;

public class Base64ToBitmapConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            if (value is not string img)
                return null;

            var bytes = System.Convert.FromBase64String(img);
            using var ms = new MemoryStream(bytes);
            return new Bitmap(ms);
        }
        catch (Exception ex)
        {
            Interactions.ShowMessageBox.Handle(new MessageDialogViewModel(ex.Message, MessageDialogIcon.Error,
                    buttons: MessageDialogButton.Ok))
                .Subscribe();

            return null;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}