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

        public ReactiveCommand<Unit, bool> Close { get; }
        
        public ReactiveCommand<Unit, Unit> SaveFile { get; }

        public ImagesCarouselDialogViewModel()
        {
            Close = ReactiveCommand.Create(() => true);

            SaveFile = ReactiveCommand.CreateFromTask(SaveSelectedImageAsync);
        }

        public ImagesCarouselDialogViewModel Configure(IEnumerable<ImageBase> images, [CanBeNull] string name = null)
        {
            Images = new ObservableCollectionExtended<ImageBase>(images);
            Name = name;
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
    }
}
