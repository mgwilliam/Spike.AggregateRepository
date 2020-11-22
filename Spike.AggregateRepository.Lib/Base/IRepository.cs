using System;
using System.Collections.Generic;

namespace Spike.AggregateRepository.Lib.Base
{
    public interface IRepository<T> where T : BaseEntity
    {
        void Create(Guid entityId, string userId);

        T Get(Guid entityId);

        T GetVersion(Guid entityId, Guid changeId);

        T GetAndApply(Guid entityId, T change);

        IEnumerable<T> GetHistory(Guid entityId);

        void Update(T entity, string userId);
    }
}
