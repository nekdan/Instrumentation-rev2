using System.Collections.Generic;

namespace Instrumentation.Model;

public sealed class Subinstument
{
    public long Id { get; set; }
    public string Name { get; set; }
    public List<Sound> Sounds { get; set; }
    public Instument Instument { get; set; }
    public long InstumentId { get; set; }
    public IEnumerable<InstrumentImage> Images { get; set; }
}