using System;

namespace Spike.AggregateRepository.Lib.Base
{
    public sealed class Change
    {
        public Guid ChangeId { get; internal set; }

        public DateTime ChangedOn { get; set; }

        public string ChangedByUser { get; set; }
    }
}
