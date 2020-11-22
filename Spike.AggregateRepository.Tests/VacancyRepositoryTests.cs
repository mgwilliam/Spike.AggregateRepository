using System;
using System.Linq;
using Spike.AggregateRepository.Lib.Infrastructure;
using Spike.AggregateRepository.Lib.Vacancies;
using Xunit;

namespace Spike.AggregateRepository.Tests
{
    public class VacancyRepositoryTests
    {
        [Fact]
        public void WhenCreating_ItShouldAddTheEntity()
        {
            var repository = new VacancyRepository(new InMemoryRepositoryStore<Vacancy>());

            var vacancyId = Guid.NewGuid();

            repository.Create(vacancyId, "user");
        }

        [Fact]
        public void WhenCreating_ItShouldSetInitialStatus()
        {
            var repository = CreateRepository();

            var vacancyId = Guid.NewGuid();

            repository.Create(vacancyId, "user");

            var vacancy = repository.Get(vacancyId);

            Assert.Equal(vacancyId, vacancy.Id);
            Assert.Equal(VacancyStatus.New, vacancy.Status);
        }

        [Fact]
        public void WhenCreating_ItShouldRejectIfTheIdIsAlreadyInUse()
        {
            var repository = CreateRepository();

            var vacancyId = Guid.NewGuid();
            repository.Create(vacancyId, "user");

            Assert.Throws<InvalidOperationException>(() => repository.Create(vacancyId, "user"));
        }

        [Fact]
        public void WhenGetting_ItShouldReturnAnEntityWithTheCorrectStatus()
        {
            var repository = CreateRepository();

            var vacancyId = Guid.NewGuid();

            repository.Create(vacancyId, "user");

            var newVacancy = repository.Get(vacancyId);

            Assert.NotNull(newVacancy);
            Assert.Equal(vacancyId, newVacancy.Id);
            Assert.Equal(VacancyStatus.New, newVacancy.Status);

            repository.Update(new Vacancy {Id = vacancyId, Title = "title", Status = VacancyStatus.Draft}, "user");
            var updatedVacancy = repository.Get(vacancyId);

            Assert.Equal(VacancyStatus.Draft, updatedVacancy.Status);
        }

        [Fact]
        public void WhenGetting_ItShouldReturnTheEntityWithAllChangesAppliedAndLastUserUpdateInfo()
        {
            var repository = CreateRepository();

            var vacancyId = Guid.NewGuid();

            repository.Create(vacancyId, "user");

            repository.Update(new Vacancy {Id = vacancyId, Title = "title", Status = VacancyStatus.Draft}, "user1");
            repository.Update(new Vacancy {Id = vacancyId, Description = "description", Status = VacancyStatus.Draft}, "user2");

            var vacancy = repository.Get(vacancyId);

            Assert.Equal(vacancyId, vacancy.Id);
            Assert.Equal("title", vacancy.Title);
            Assert.Equal("description", vacancy.Description);
            Assert.Equal(VacancyStatus.Draft, vacancy.Status);
            Assert.Equal("user2", vacancy.ChangeInfo.ChangedByUser);
            Assert.NotEqual(DateTime.MinValue, vacancy.ChangeInfo.ChangedOn);
        }

        [Fact]
        public void WhenGetting_ItShouldRejectIfTheIdIsNotInTheStore()
        {
            var repository = CreateRepository();

            var vacancyId = Guid.NewGuid();

            Assert.Throws<InvalidOperationException>(() => repository.Get(vacancyId));
        }

        [Fact]
        public void WhenGettingByVersion_ItShouldReturnTheEntityAtTheSpecifiedVersion()
        {
            var repository = CreateRepository();

            var vacancyId = Guid.NewGuid();

            repository.Create(vacancyId, "user1");
            repository.Update(new Vacancy {Id = vacancyId, Title = "title", Status = VacancyStatus.Draft}, "user2");
            repository.Update(new Vacancy {Id = vacancyId, Description = "description", Status = VacancyStatus.Draft}, "user3");

            var vacancyHistory = repository.GetHistory(vacancyId);

            var versionId = vacancyHistory.ElementAt(1).ChangeInfo.ChangeId;

            var vacancy = repository.GetVersion(vacancyId, versionId);

            Assert.Equal("title", vacancy.Title);
            Assert.Null(vacancy.Description);
            Assert.Equal("user2", vacancy.ChangeInfo.ChangedByUser);
        }

        [Fact]
        public void WhenGettingByVersion_ItShouldRejectIfTheVersionIdIsNotInTheStore()
        {
            var repository = CreateRepository();

            var vacancyId = Guid.NewGuid();

            repository.Create(vacancyId, "user1");
            repository.Update(new Vacancy {Id = vacancyId, Title = "title", Status = VacancyStatus.Draft}, "user2");
            repository.Update(new Vacancy {Id = vacancyId, Description = "description", Status = VacancyStatus.Draft}, "user3");

            var nonExistentVersionId = Guid.NewGuid();

            Assert.Throws<InvalidOperationException>(() => repository.GetVersion(vacancyId, nonExistentVersionId));
        }

        [Fact]
        public void WhenGettingAndApplying_ItShouldReturnTheSavedEntityAndApplySpecifiedChanges()
        {
            var repository = CreateRepository();

            var vacancyId = Guid.NewGuid();

            repository.Create(vacancyId, "user1");
            repository.Update(new Vacancy {Id = vacancyId, Title = "title", Status = VacancyStatus.Draft}, "user2");
            repository.Update(new Vacancy {Id = vacancyId, Description = "description", Status = VacancyStatus.Draft}, "user3");

            AssertSavedVacancy(repository.Get(vacancyId));

            var unsavedChange = new Vacancy {Title = "new title", Description = "new description", Status = VacancyStatus.Draft};
            var vacancyWithUnsavedChange = repository.GetAndApply(vacancyId, unsavedChange);

            Assert.Equal(vacancyId, vacancyWithUnsavedChange.Id);
            Assert.Equal("new title", vacancyWithUnsavedChange.Title);
            Assert.Equal("new description", vacancyWithUnsavedChange.Description);
            Assert.Equal("user3", vacancyWithUnsavedChange.ChangeInfo.ChangedByUser);
            Assert.Equal(VacancyStatus.Draft, vacancyWithUnsavedChange.Status);

            // ensure the saved entity hasn't been changed
            AssertSavedVacancy(repository.Get(vacancyId));

            void AssertSavedVacancy(Vacancy vacancy)
            {
                Assert.Equal("title", vacancy.Title);
                Assert.Equal("description", vacancy.Description);
                Assert.Equal("user3", vacancy.ChangeInfo.ChangedByUser);
            }
        }

        [Fact]
        public void WhenGettingAndApplying_ItShouldRejectIfTheIdIsNotInTheStore()
        {
            var repository = CreateRepository();

            var nonExistentVersionId = Guid.NewGuid();
            var unsavedChange = new Vacancy {Title = "title"};

            Assert.Throws<InvalidOperationException>(() => repository.GetAndApply(nonExistentVersionId, unsavedChange));
        }

        [Fact]
        public void WhenGettingHistory_ItShouldReturnEveryChangeToTheEntityInChronologicalOrder()
        {
            var repository = CreateRepository();

            var vacancyId = Guid.NewGuid();

            repository.Create(vacancyId, "user1");

            repository.Update(new Vacancy {Id = vacancyId, Title = "title", Status = VacancyStatus.Draft}, "user2");
            repository.Update(new Vacancy {Id = vacancyId, Description = "description", Status = VacancyStatus.Draft}, "user3");

            var vacancyHistory = repository.GetHistory(vacancyId).ToArray();

            Assert.Equal(3, vacancyHistory.Length);

            Assert.Equal("user1", vacancyHistory.ElementAt(0).ChangeInfo.ChangedByUser);
            Assert.Equal("user2", vacancyHistory.ElementAt(1).ChangeInfo.ChangedByUser);
            Assert.Equal("user3", vacancyHistory.ElementAt(2).ChangeInfo.ChangedByUser);
        }

        [Fact]
        public void WhenGettingHistory_EachChangeShouldOnlyContainPropertiesWithValuesSpecifiedWhenItWasUpdated()
        {
            var repository = CreateRepository();

            var vacancyId = Guid.NewGuid();

            repository.Create(vacancyId, "user");

            repository.Update(new Vacancy {Id = vacancyId, Title = "title1", Description = "description1"}, "user");
            repository.Update(new Vacancy {Id = vacancyId, Title = "title2"}, "user");
            repository.Update(new Vacancy {Id = vacancyId, Description = "description2"}, "user");

            var vacancyHistory = repository.GetHistory(vacancyId).ToArray();

            Assert.Equal("title1", vacancyHistory.ElementAt(1).Title);
            Assert.Equal("description1", vacancyHistory.ElementAt(1).Description);

            Assert.Equal("title2", vacancyHistory.ElementAt(2).Title);
            Assert.Null(vacancyHistory.ElementAt(2).Description);

            Assert.Null(vacancyHistory.ElementAt(3).Title);
            Assert.Equal("description2", vacancyHistory.ElementAt(3).Description);
        }

        [Fact]
        public void WhenUpdating_ItShouldRejectIfTheIdIsNotInTheStore()
        {
            var repository = CreateRepository();

            var vacancyId = Guid.NewGuid();

            var update = new Vacancy {Id = vacancyId, Title = "title"};

            Assert.Throws<InvalidOperationException>(() => repository.Update(update, "user"));
        }

        [Fact]
        public void WhenUpdating_ItShouldEnsureThatTheEntityIdIsPopulated()
        {
            var repository = CreateRepository();

            var vacancyId = Guid.NewGuid();

            repository.Create(vacancyId, "user");

            Assert.Throws<InvalidOperationException>(() => repository.Update(new Vacancy {Title = "title"}, "user"));
        }

        private static IVacancyRepository CreateRepository()
        {
            return new VacancyRepository(new InMemoryRepositoryStore<Vacancy>());
        }
    }
}
