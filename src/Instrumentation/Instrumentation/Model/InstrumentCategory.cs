using System.Collections.Generic;

namespace Instrumentation.Model;

public class InstrumentCategory
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string IconBase64 { get; set; }
    public List<Instument> Instuments { get; set; }
}