using System;

namespace Spike.AggregateRepository.Lib.Vacancies
{
    public interface IVacancyService
    {
        void Create(Guid vacancyId, string userId);

        Vacancy Get(Guid vacancyId);

        void Save(Vacancy vacancy, string userId);

        void Approve(Guid vacancyId, string userId);
    }
}
