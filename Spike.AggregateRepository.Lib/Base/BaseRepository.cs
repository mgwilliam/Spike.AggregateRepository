using System;
using System.Collections.Generic;
using System.Linq;

namespace Spike.AggregateRepository.Lib.Base
{
    public abstract class BaseRepository<T> : IRepository<T> where T : BaseEntity, new()
    {
        private readonly IRepositoryStore<T> _store;

        protected BaseRepository(IRepositoryStore<T> store)
        {
            _store = store;
        }

        public void Create(Guid entityId, string userId)
        {
            var changeId = Guid.NewGuid();

            var instance = new T
            {
                Id = entityId,
                ChangeInfo = new Change {ChangeId = changeId, ChangedByUser = userId, ChangedOn = DateTime.Now}
            };

            InitialiseInstance(instance);

            _store.CreateNew(instance);
        }

        protected abstract void InitialiseInstance(T instance);

        public T Get(Guid entityId)
        {
            var changes = _store.GetChanges(entityId).ToArray();

            var instance = changes.First();

            foreach (var change in changes.Skip(1))
            {
                ApplyChange(instance, change);
            }

            return instance;
        }

        public T GetAndApply(Guid entityId, T change)
        {
            var instance = Get(entityId);

            ApplyChange(instance, change);

            return instance;
        }

        public IEnumerable<T> GetHistory(Guid entityId)
        {
            return _store.GetChanges(entityId).ToArray();
        }

        public T GetVersion(Guid entityId, Guid changeId)
        {
            var changes = _store.GetChanges(entityId).ToArray();

            if (changes.All(c => c.ChangeInfo.ChangeId != changeId))
                throw new InvalidOperationException($"Cannot get entity '{entityId:D}' with change version '{changeId:D}' as it doesn't exist");

            var instance = changes.First();

            foreach (var change in changes.Skip(1))
            {
                ApplyChange(instance, change);
                if (change.ChangeInfo.ChangeId == changeId) break;
            }

            return instance;
        }

        public void Update(T entity, string userId)
        {
            var changeId = Guid.NewGuid();

            entity.ChangeInfo = new Change {ChangeId = changeId, ChangedByUser = userId, ChangedOn = DateTime.Now};

            _store.AddChange(entity);
        }

        protected abstract void ApplyChange(T instance, T change);
    }
}
