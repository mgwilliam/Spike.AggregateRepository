using System;
using System.ComponentModel.DataAnnotations;

namespace Spike.AggregateRepository.Lib.Vacancies
{
    public static class VacancyValidator
    {
        public static void Validate(Vacancy instance)
        {
            // extremely simple and contrived validation class

            if (string.IsNullOrWhiteSpace(instance.Title))
                throw new ValidationException("Title can't be empty");

            if (string.IsNullOrWhiteSpace(instance.Description))
                throw new ValidationException("Description can't be empty");

            if (instance.Description.Equals(instance.Title))
                throw new ValidationException("Title and Description can't be the same");
        }
    }
}
