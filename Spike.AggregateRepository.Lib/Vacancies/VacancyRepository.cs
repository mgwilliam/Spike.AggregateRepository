using System;
using Spike.AggregateRepository.Lib.Base;

namespace Spike.AggregateRepository.Lib.Vacancies
{
    public class VacancyRepository : BaseRepository<Vacancy>, IVacancyRepository
    {
        public VacancyRepository(IRepositoryStore<Vacancy> store) : base(store) { }

        protected override void InitialiseInstance(Vacancy instance)
        {
            instance.Status = VacancyStatus.New;
        }

        protected override void ApplyChange(Vacancy instance, Vacancy change)
        {
            instance.Title = change.Title ?? instance.Title;
            instance.Description = change.Description ?? instance.Description;
            instance.Status = change.Status;
            instance.ChangeInfo = change.ChangeInfo ?? instance.ChangeInfo;
        }
    }
}
