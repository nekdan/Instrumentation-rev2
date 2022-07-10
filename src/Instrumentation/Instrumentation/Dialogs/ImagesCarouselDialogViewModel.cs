using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData.Binding;
using Instrumentation.Model;
using Instrumentation.ViewModels;
using JetBrains.Annotations;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Instrumentation.Dialogs
{
    public class ImagesCarouselDialogViewModel : ViewModelBase
    {
        [Reactive] public ObservableCollectionExtended<ImageBase> Images { get; private set; } = new();

        [Reactive] public string Name { get; private set; }

        [Reactive] public ImageBase SelectedImage { get; set; }

        [Reactive] public bool CanSaveAllImages { get; private set; }

        public ReactiveCommand<Unit, bool> Close { get; }

        public ReactiveCommand<Unit, Unit> SaveFile { get; }

        public ReactiveCommand<Unit, Unit> SaveAllFiles { get; }

        public ImagesCarouselDialogViewModel()
        {
            Close = ReactiveCommand.Create(() => true);

            SaveFile = ReactiveCommand.CreateFromTask(SaveSelectedImageAsync);

            SaveAllFiles = ReactiveCommand.CreateFromTask(SaveAllImagesAsync);
        }

        public ImagesCarouselDialogViewModel Configure(IEnumerable<ImageBase> images, [CanBeNull] string name = null)
        {
            Images = new ObservableCollectionExtended<ImageBase>(images);
            Name = name;

            CanSaveAllImages = Images.Count > 1;
            return this;
        }

        private async Task SaveSelectedImageAsync()
        {
            var filePath = await Interactions.ShowSaveFileDialog.Handle(Name);

            if (string.IsNullOrWhiteSpace(filePath))
            {
                //await Interactions.ShowMessageBox.Handle(new MessageDialogViewModel("Ошибка сохранения картинки", MessageDialogIcon.Error));

                return;
            }

            if (string.IsNullOrWhiteSpace(Path.GetExtension(filePath)))
                filePath = Path.ChangeExtension(filePath, "jpg");

            if(File.Exists(filePath))
                File.Delete(filePath);

            var bytes = Convert.FromBase64String(SelectedImage.ImageBase64);
            using var ms = new MemoryStream(bytes);
            await using var fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite);
            ms.WriteTo(fs);
            fs.Flush();
        }

        private async Task SaveAllImagesAsync()
        {
            var filePath = await Interactions.ShowSelectDirectoryDialog.Handle(Unit.Default);

            if (string.IsNullOrWhiteSpace(filePath))
                return;

            for(var i = 0 ; i < Images.Count; i++)
            {
                var image = Images[i];
                Name = string.Join("_", Name.Split(Path.GetInvalidFileNameChars()));
                filePath = Path.Combine(filePath, $"{Name}-{i + 1}");
                filePath = Path.ChangeExtension(filePath, "jpg");

                if (File.Exists(filePath))
                    File.Delete(filePath);

                var bytes = Convert.FromBase64String(image.ImageBase64);
                using var ms = new MemoryStream(bytes);
                await using var fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite);
                ms.WriteTo(fs);
                fs.Flush();
            }
        }
    }
}
