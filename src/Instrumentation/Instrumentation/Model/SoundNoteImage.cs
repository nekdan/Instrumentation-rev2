using JetBrains.Annotations;

namespace Instrumentation.Model;

public sealed class SoundNoteImage : ImageBase
{
    [CanBeNull] public Sound Sound { get; set; }
    [CanBeNull] public Subsound Subsound { get; set; }
    public long? SoundId { get; set; }
    public long? SubsoundId { get; set; }
}