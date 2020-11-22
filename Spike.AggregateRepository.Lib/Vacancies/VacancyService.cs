using System;

namespace Spike.AggregateRepository.Lib.Vacancies
{
    public class VacancyService : IVacancyService
    {
        private readonly IVacancyRepository _vacancyRepository;

        public VacancyService(IVacancyRepository vacancyRepository)
        {
            _vacancyRepository = vacancyRepository;
        }

        public void Create(Guid vacancyId, string userId)
        {
            _vacancyRepository.Create(vacancyId, userId);
        }

        public Vacancy Get(Guid vacancyId)
        {
            return _vacancyRepository.Get(vacancyId);
        }

        public void Save(Vacancy vacancy, string userId)
        {
            var savedVacancy = _vacancyRepository.Get(vacancy.Id);

            if (savedVacancy.Status == VacancyStatus.Approved)
                throw new InvalidOperationException($"Cannot update entity '{savedVacancy.Id:D}' as the status is '{savedVacancy}'");

            vacancy.Status = VacancyStatus.Draft;

            Validate(vacancy);

            _vacancyRepository.Update(vacancy, userId);
        }

        public void Approve(Guid vacancyId, string userId)
        {
            var vacancy = _vacancyRepository.Get(vacancyId);

            if (vacancy.Status != VacancyStatus.Draft)
                throw new InvalidOperationException($"Cannot approve entity '{vacancyId:D}' as the status is '{vacancy.Status}'");

            var change = new Vacancy
            {
                Id = vacancyId,
                Status = VacancyStatus.Approved,
            };

            _vacancyRepository.Update(change, userId);
        }

        private void Validate(Vacancy change)
        {
            // apply change to saved entity
            var instance = _vacancyRepository.GetAndApply(change.Id, change);

            VacancyValidator.Validate(instance);
        }
    }
}
