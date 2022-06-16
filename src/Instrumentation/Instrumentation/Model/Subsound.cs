using System.Collections.Generic;

namespace Instrumentation.Model;

public class Subsound
{
    public long Id { get; set; }
    public string Name { get; set; }
    public List<SoundNoteImage> SoundNoteImages { get; set; } = new();
    public SoundData SoundData { get; set; }
    public Sound Sound { get; set; }
    public long SoundId { get; set; }
}