using System;
using System.ComponentModel.DataAnnotations;
using Spike.AggregateRepository.Lib.Infrastructure;
using Spike.AggregateRepository.Lib.Vacancies;
using Xunit;

namespace Spike.AggregateRepository.Tests
{
    public class VacancyServiceTests
    {
        [Fact]
        public void WhenSaving_ItShouldRejectIfTheVacancyIsNotValid()
        {
            var service = CreateService();

            var vacancyId = Guid.NewGuid();

            service.Create(vacancyId, "user");

            var update = new Vacancy {Id = vacancyId, Title = "title"};

            Assert.Throws<ValidationException>(() => service.Save(update, "user"));
        }

        [Fact]
        public void WhenSaving_ItShouldRejectIfTheCurrentStatusIsNotCompatible()
        {
            var service = CreateService();

            var vacancyId = Guid.NewGuid();
            
            service.Create(vacancyId, "user");
            service.Save(new Vacancy {Id = vacancyId, Title = "title", Description = "description"}, "user");

            service.Approve(vacancyId, "user");

            var update = new Vacancy {Id = vacancyId, Title = "title"};

            Assert.Throws<InvalidOperationException>(() => service.Save(update, "user"));
        }

        [Fact]
        public void WhenApproving_ItShouldSetTheStatusToApproved()
        {
            var service = CreateService();

            var vacancyId = Guid.NewGuid();

            service.Create(vacancyId, "user");
            service.Save(new Vacancy {Id = vacancyId, Title = "title", Description = "description"}, "user");

            Assert.Equal(VacancyStatus.Draft, service.Get(vacancyId).Status);

            service.Approve(vacancyId, "user");

            Assert.Equal(VacancyStatus.Approved, service.Get(vacancyId).Status);
        }

        [Fact]
        public void WhenApproving_ItShouldRejectIfTheCurrentStatusIsNotCompatible()
        {
            var service = CreateService();

            var vacancyId = Guid.NewGuid();

            service.Create(vacancyId, "user");
            service.Save(new Vacancy {Id = vacancyId, Title = "title", Description = "description"}, "user");
            service.Approve(vacancyId, "user");

            Assert.Equal(VacancyStatus.Approved, service.Get(vacancyId).Status);

            Assert.Throws<InvalidOperationException>(() => service.Approve(vacancyId, "user"));
        }

        private static IVacancyService CreateService()
        {
            return new VacancyService(new VacancyRepository(new InMemoryRepositoryStore<Vacancy>()));
        }
    }
}
