using System.Collections.Generic;
using JetBrains.Annotations;
//using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Instrumentation.Model;

public sealed class SoundData
{
    public long Id { get; set; }
    public string Description { get; set; }
    public string SoundBase64 { get; set; }
    public List<SoundNoteImage> NoteImages { get; set; }
    [CanBeNull] public Sound Sound { get; set; }
    [CanBeNull] public Subsound Subsound { get; set; }
    public long? SoundId { get; set; }
    public long? SubsoundId { get; set; }
    public string SoundName => Sound?.Name ?? Subsound?.Name ?? string.Empty;
}