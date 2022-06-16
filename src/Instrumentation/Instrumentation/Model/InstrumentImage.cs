using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instrumentation.Model
{
    public sealed class InstrumentImage : ImageBase
    {
        public long? InstumentId { get; set; }
        public Instument Instument { get; set; }
        public Subinstument Subinstument { get; set; }
        public long? SubinstumentId { get; set; }
    }
}
