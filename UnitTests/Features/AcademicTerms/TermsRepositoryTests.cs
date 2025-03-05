using Moq;
using StudentUsos.Features.AcademicTerms.Models;
using StudentUsos.Features.AcademicTerms.Repositories;
using StudentUsos.Services.LocalDatabase;

namespace UnitTests.Features.AcademicTerms;

[Category(Categories.Unit_SharedBusinessLogic)]
public class TermsRepositoryTests
{
    [Fact]
    public void TryGettingCurrentTerm_WhenCurrentTermExists_ReturnsTrueAndFindsTerm()
    {
        // Arrange
        var activeTerm = new Term
        {
            StartDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"),
            FinishDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"),
        };
        List<Term> terms = new()
        {
            activeTerm,
            new Term
            {
                StartDate = DateTime.Now.AddMonths(-12).ToString("yyyy-MM-dd"),
                FinishDate = DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd"),
            },
            new Term
            {
                StartDate = DateTime.Now.AddMonths(6).ToString("yyyy-MM-dd"),
                FinishDate = DateTime.Now.AddMonths(12).ToString("yyyy-MM-dd"),
            }
        };

        var localDatabaseMock = new Mock<ILocalDatabaseManager>();
        localDatabaseMock.Setup(mock => mock.GetAll<Term>()).Returns(terms);

        var termsRepository = new TermsRepository(localDatabaseMock.Object);

        // Act
        var result = termsRepository.TryGettingCurrentTerm(out var currentTerm);

        // Assert
        Assert.True(result);
        Assert.Equal(activeTerm, currentTerm);
    }
}