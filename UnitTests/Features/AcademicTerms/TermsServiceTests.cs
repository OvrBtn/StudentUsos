#pragma warning disable 8625
using Moq;
using StudentUsos.Features.AcademicTerms.Models;
using StudentUsos.Features.AcademicTerms.Repositories;
using StudentUsos.Features.AcademicTerms.Services;
using StudentUsos.Services.ServerConnection;
using System.Text.Json;

namespace UnitTests.Features.AcademicTerms
{
    [Category(Categories.Unit_SharedBusinessLogic)]
    public class TermsServiceTests
    {
        [Fact]
        public async Task GetTermsAsync_WhenValidResponse_ReturnsTerms()
        {
            //Arrange
            var json = MockDataHelper.LoadFile("TermsSearch.json", "AcademicTerms");
            var jsonDeserialized = JsonSerializer.Deserialize<List<Term>>(json);
            var jsonDeserializedFiltered = jsonDeserialized!.Where(x => x.Id.ToLower().Contains("l") || x.Id.ToLower().Contains("z"));

            var serverConnectionMock = new Mock<IServerConnectionManager>();
            ServerConnectionManagerTestHelper.SetupGenericSendRequestToUsos(serverConnectionMock, json);

            var termsService = new TermsService(serverConnectionMock.Object, null);

            //Act

            //filtering terms based on start and end date happens on usos api side
            var terms = await termsService.GetTermsAsync(DateTime.MinValue, DateTime.MaxValue);

            //Assert
            Assert.NotNull(terms);
            Assert.All(terms, term =>
            {
                Assert.True(term.Id.ToLower().Contains("l") || term.Id.ToLower().Contains("z"));
            });
            Assert.Equal(jsonDeserializedFiltered.Count(), terms.Count);

        }

        [Fact]
        public async Task GetTermsAsync_WhenInvalidResponse_ReturnsNull()
        {
            //Arrange
            var serverConnectionMock = new Mock<IServerConnectionManager>();
            ServerConnectionManagerTestHelper.SetupGenericSendRequestToUsos(serverConnectionMock, null);

            var termsService = new TermsService(serverConnectionMock.Object, null);

            //Act
            var terms = await termsService.GetTermsAsync(DateTime.MinValue, DateTime.MaxValue);

            //Assert
            Assert.Null(terms);
        }

        [Fact]
        public async Task GetCurrentTermAsync_WhenValidResponse_ReturnsTerm()
        {
            //Arrange
            var currentTerm = new Term
            {
                Id = "2023Z",
                StartDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"),
                FinishDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"),
            };
            List<Term> terms = new()
            {
                currentTerm,
                new Term
                {
                    Id = "2023L",
                    StartDate = DateTime.Now.AddMonths(-12).ToString("yyyy-MM-dd"),
                    FinishDate = DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd"),
                },
                new Term
                {
                    Id = "2024Z",
                    StartDate = DateTime.Now.AddMonths(6).ToString("yyyy-MM-dd"),
                    FinishDate = DateTime.Now.AddMonths(12).ToString("yyyy-MM-dd"),
                }
            };
            var serverConnectionMock = new Mock<IServerConnectionManager>();
            ServerConnectionManagerTestHelper.SetupGenericSendRequestToUsos(serverConnectionMock, JsonSerializer.Serialize(terms));
            var termsRepositoryMock = new Mock<ITermsRepository>();

            var termsService = new TermsService(serverConnectionMock.Object, termsRepositoryMock.Object);

            //Act
            var term = await termsService.GetCurrentTermAsync();

            //Assert
            Assert.NotNull(term);
            Assert.Equal(JsonSerializer.Serialize(currentTerm), JsonSerializer.Serialize(term));
        }

        [Fact]
        public async Task GetCurrentTermAsync_WhenInvalidResponse_ReturnsNull()
        {
            //Arrange
            List<Term> terms = new()
            {
                new Term
                {
                    Id = "2023L",
                    StartDate = DateTime.Now.AddMonths(-12).ToString("yyyy-MM-dd"),
                    FinishDate = DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd"),
                },
                new Term
                {
                    Id = "2024Z",
                    StartDate = DateTime.Now.AddMonths(6).ToString("yyyy-MM-dd"),
                    FinishDate = DateTime.Now.AddMonths(12).ToString("yyyy-MM-dd"),
                }
            };
            var serverConnectionMock = new Mock<IServerConnectionManager>();
            ServerConnectionManagerTestHelper.SetupGenericSendRequestToUsos(serverConnectionMock, JsonSerializer.Serialize(terms));
            var termsRepositoryMock = new Mock<ITermsRepository>();

            var termsService = new TermsService(serverConnectionMock.Object, termsRepositoryMock.Object);

            //Act
            var term = await termsService.GetCurrentTermAsync();

            //Assert
            Assert.Null(term);
        }
    }
}
