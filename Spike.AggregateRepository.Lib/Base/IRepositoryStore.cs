using System;
using System.Collections.Generic;

namespace Spike.AggregateRepository.Lib.Base
{
    public interface IRepositoryStore<T> where T : BaseEntity
    {
        void CreateNew(T entity);

        IEnumerable<T> GetChanges(Guid entityId);

        void AddChange(T entity);
    }
}
