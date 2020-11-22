using System;
using System.Collections.Generic;
using Spike.AggregateRepository.Lib.Base;

namespace Spike.AggregateRepository.Lib.Infrastructure
{
    public class InMemoryRepositoryStore<T> : IRepositoryStore<T> where T : BaseEntity
    {
        private readonly Dictionary<Guid, List<T>> _instances;

        public InMemoryRepositoryStore()
        {
            _instances = new Dictionary<Guid, List<T>>();
        }

        public void CreateNew(T entity)
        {
            if (_instances.ContainsKey(entity.Id))
                throw new InvalidOperationException($"Cannot create entity with ID '{entity.Id:D}' as that ID is already in use");

            _instances.Add(entity.Id, new List<T>(new[] {entity}));
        }

        public IEnumerable<T> GetChanges(Guid entityId)
        {
            if (!_instances.ContainsKey(entityId))
                throw new InvalidOperationException($"Cannot get entity with unrecognized ID '{entityId:D}'");

            return _instances[entityId];
        }

        public void AddChange(T entity)
        {
            if (!_instances.ContainsKey(entity.Id))
                throw new InvalidOperationException($"Cannot update entity with unrecognized ID '{entity.Id:D}'");

            var changes = _instances[entity.Id];
            changes.Add(entity);
        }
    }
}
