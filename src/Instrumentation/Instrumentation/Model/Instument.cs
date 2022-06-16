using System.Collections.Generic;
using System.Linq;

namespace Instrumentation.Model;

public sealed class Instument
{
    public long Id { get; set; }
    public string Name { get; set; }
    public InstrumentCategory Category { get; set; }
    public long CategoryId { get; set; }
    public List<Subinstument> Subinstuments { get; set; }
    public List<Sound> Sounds { get; set; }
    public List<InstrumentImage> Images { get; set; }
    public bool IsInstumentGroup => Subinstuments.Any();
}