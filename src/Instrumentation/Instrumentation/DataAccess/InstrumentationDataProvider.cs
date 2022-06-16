using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Instrumentation.Model;
using Instrumentation.Model.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Instrumentation.DataAccess;

public sealed class InstrumentationDataProvider : IInstrumentationDataProvider
{
    private readonly InstrumentationContext _dbContext;

    public InstrumentationDataProvider(InstrumentationContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async IAsyncEnumerable<InstrumentCategory> GetInstrumentCategoriesAsync()
    {
        var loop = _dbContext.Categories
            .AsNoTracking()
            .AsAsyncEnumerable();

        await foreach (var item in loop)
            yield return item;
    }

    public async IAsyncEnumerable<Instument> GetInstrumentsAsync(InstrumentCategory instrumentCategory)
    {
        var loop = _dbContext.Instuments
            .Include(x => x.Subinstuments)
            .Where(x => x.CategoryId == instrumentCategory.Id)
            .AsNoTracking()
            .AsAsyncEnumerable();

        await foreach (var item in loop)
            yield return item;
    }

    public async IAsyncEnumerable<Sound> GetSoundsAsync(Instument instument)
    {
        var loop = _dbContext.Sounds
            .Include(x => x.Subsounds)
            .Where(x => x.InstumentId != null)
            .Where(x => x.InstumentId == instument.Id)
            .AsNoTracking()
            .AsAsyncEnumerable();

        await foreach (var item in loop)
            yield return item;
    }

    public async IAsyncEnumerable<Sound> GetSoundsAsync(Subinstument subinstument)
    {
        var loop = _dbContext.Sounds
            .Include(x => x.Subsounds)
            .Where(x => x.SubinstumentId != null)
            .Where(x => x.SubinstumentId == subinstument.Id)
            .AsNoTracking()
            .AsAsyncEnumerable();

        await foreach (var item in loop)
            yield return item;
    }

    public async Task<SoundData> GetSoundDataAsync(Sound sound)
    {
       var data = await _dbContext.SoundsDatas
            .AsNoTracking()
            .Where(x => x.SoundId != null)
            .Include(x => x.Sound)
            .SingleOrDefaultAsync(x => x.SoundId == sound.Id);

       if (data is not null)
       {
           await FillSoundDataNotesAsync(data, sound);
       }
       
       return data;
    }
    
    public async Task<SoundData> GetSoundDataAsync(Subsound subsound)
    {
        var data = await _dbContext.SoundsDatas
            .AsNoTracking()
            .Where(x => x.SubsoundId != null)
            .Include(x => x.Subsound)
            .SingleOrDefaultAsync(x => x.SubsoundId == subsound.Id);

        if (data is not null)
        {
            await FillSoundDataNotesAsync(data, subsound);
        }

        return data;
    }

    public async IAsyncEnumerable<InstrumentImage> GetInstrumentImagesAsync(Instument instument)
    {
        var data = _dbContext.InstrumentImages
            .AsNoTracking()
            .Where(x => x.InstumentId == instument.Id)
            .AsAsyncEnumerable();

        await foreach (var item in data)
            yield return item;
    }

    public async IAsyncEnumerable<InstrumentImage> GetSubInstrumentImagesAsync(Subinstument subinstument)
    {
        var data = _dbContext.InstrumentImages
            .AsNoTracking()
            .Where(x => x.SubinstumentId == subinstument.Id)
            .AsAsyncEnumerable();

        await foreach (var item in data)
            yield return item;
    }

    private async Task FillSoundDataNotesAsync(SoundData soundData, Subsound subsound)
    {
        soundData.NoteImages = await _dbContext.NoteImages
            .AsNoTracking()
            .Where(x => x.SubsoundId == subsound.Id)
            .ToListAsync();
    }

    private async Task FillSoundDataNotesAsync(SoundData soundData, Sound sound)
    {
        soundData.NoteImages = await _dbContext.NoteImages
            .AsNoTracking()
            .Where(x => x.SoundId == sound.Id)
            .ToListAsync();
    }
}