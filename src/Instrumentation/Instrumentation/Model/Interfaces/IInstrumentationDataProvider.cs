using System.Collections.Generic;
using System.Threading.Tasks;

namespace Instrumentation.Model.Interfaces;

public interface IInstrumentationDataProvider
{
    IAsyncEnumerable<InstrumentCategory> GetInstrumentCategoriesAsync();

    IAsyncEnumerable<Instument> GetInstrumentsAsync(InstrumentCategory instrumentCategory);

    IAsyncEnumerable<Sound> GetSoundsAsync(Instument instument);

    IAsyncEnumerable<Sound> GetSoundsAsync(Subinstument subinstument);

    Task<SoundData> GetSoundDataAsync(Sound sound);

    Task<SoundData> GetSoundDataAsync(Subsound subsound);

    IAsyncEnumerable<InstrumentImage> GetInstrumentImagesAsync(Instument instument);

    IAsyncEnumerable<InstrumentImage> GetSubInstrumentImagesAsync(Subinstument subinstument);
}