using System;

namespace Spike.AggregateRepository.Lib.Base
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }

        public Change ChangeInfo { get; set; }
    }
}
