using System.Reactive;
using Instrumentation.Dialogs;
using ReactiveUI;

namespace Instrumentation
{
    public static class Interactions
    {
        public static Interaction<MessageDialogViewModel, MessageDialogResult> ShowMessageBox = new();

        public static Interaction<ImagesCarouselDialogViewModel, Unit> ShowImagesCarousel = new();

        public static Interaction<string, string> ShowSaveFileDialog = new();

        public static Interaction<Unit, string> ShowSelectDirectoryDialog = new();
    }
}
