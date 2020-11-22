using System;
using Spike.AggregateRepository.Lib.Base;

namespace Spike.AggregateRepository.Lib.Vacancies
{
    public class Vacancy : BaseEntity
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public VacancyStatus Status { get; set; }
    }

    public enum VacancyStatus
    {
        New = 0,
        Draft,
        Approved
    }
}
