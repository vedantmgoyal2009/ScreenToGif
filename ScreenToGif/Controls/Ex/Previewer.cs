using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ScreenToGif.Controls.Ex;

public class Previewer : Image 
{
    //This previewer needs to be pretty simple (?).
    //The pan/zoom controls will be handled by the renderer in the ViewModel.
    //Decorators (ruber band selection and other interaction controls) will be displayed on top of the image.
    //The ViewModel will handle the sync between both.


    //Maybe use this, or just use the actual Source from the image itself.
    public static readonly DependencyProperty RenderedImageProperty = DependencyProperty.Register(nameof(RenderedImage), typeof(WriteableBitmap), typeof(Previewer), new PropertyMetadata(default(WriteableBitmap)));

    public WriteableBitmap RenderedImage
    {
        get => (WriteableBitmap)GetValue(RenderedImageProperty);
        set => SetValue(RenderedImageProperty, value);
    }
}