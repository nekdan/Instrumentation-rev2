using System.Collections.Generic;
using JetBrains.Annotations;

namespace Instrumentation.Model;

public sealed class Sound
{
    public long Id { get; set; }
    public string Name { get; set; }
    public SoundData SoundData { get; set; }
    public List<Subsound> Subsounds { get; set; } = new();
    public List<SoundNoteImage> SoundNoteImages { get; set; } = new();
    [CanBeNull] public Instument Instument { get; set; }
    [CanBeNull] public Subinstument Subinstument { get; set; }
    public long? InstumentId { get; set; }
    public long? SubinstumentId { get; set; }
    public bool IsSoundGroup => Subsounds.Count > 0;
}