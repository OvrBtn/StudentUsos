using Moq;
using StudentUsos.Features.Activities.Models;
using StudentUsos.Features.Activities.Repositories;
using StudentUsos.Features.Activities.Services;
using StudentUsos.Features.Activities.Views;
using StudentUsos.ViewModels;

namespace UnitTests.Features.Activities;

[Category(Categories.Unit_SharedBusinessLogic)]
public class ActivitiesViewModelTests
{
    List<Activity> currentAndValidActivities = new();
    List<TimetableDay> currentAndValidDays = new();
    List<Activity> futureAndValidActivities = new();
    List<TimetableDay> futureAndValidDays = new();
    public ActivitiesViewModelTests()
    {
        for (int i = 0; i < 2; i++)
        {
            currentAndValidActivities.Add(new()
            {
                StartDateTime = DateTime.Now.AddHours(i * 2),
                EndDateTime = DateTime.Now.AddHours(i * 2 + 1)
            });
        }
        for (int i = 0; i < 3; i++)
        {
            currentAndValidDays.Add(new(DateTime.Now.AddDays(i))
            {
                Activities = currentAndValidActivities,
                CreationDate = DateTime.Now,
            });
        }

        for (int i = 0; i < 2; i++)
        {
            futureAndValidActivities.Add(new()
            {
                StartDateTime = DateTime.Now.AddHours(i * 2),
                EndDateTime = DateTime.Now.AddHours(i * 2 + 1)
            });
        }
        for (int i = 0; i < 3; i++)
        {
            futureAndValidDays.Add(new(DateTime.Now.AddDays(i + 14))
            {
                Activities = currentAndValidActivities,
                CreationDate = DateTime.Now,
            });
        }
    }

    [Fact]
    public void ActivitiesViewModel_WhenValidActivitiesInDatabase_Assigns()
    {
        //Arrange
        var activitiesRepoMock = new Mock<IActivitiesRepository>();
        activitiesRepoMock.Setup(m => m.GetAllActivities())
            .Returns(new GetActivitiesResult(currentAndValidDays, currentAndValidActivities));

        var viewModel = new ActivitiesViewModel(new ApplicationServiceMock(), activitiesRepoMock.Object, new Mock<IActivitiesService>().Object);

        //Act
        viewModel.Init();

        //Assert
        Assert.True(viewModel.ObservableDays.ContainsKey(DateOnly.FromDateTime(DateTime.Now)));
        Assert.True(viewModel.ObservableDays.ContainsKey(DateOnly.FromDateTime(DateTime.Now.AddDays(1))));
        Assert.True(viewModel.ObservableDays.ContainsKey(DateOnly.FromDateTime(DateTime.Now.AddDays(2))));
        Assert.Equal(BaseViewModel.StateKey.Loaded, viewModel.MainStateKey);
    }

    [Fact]
    public void ActivitiesViewModel_WhenInvalidActivitiesInDatabase_FetchesFromServerAndAssigns()
    {
        //Arrange
        var activitiesRepoMock = new Mock<IActivitiesRepository>();
        activitiesRepoMock.Setup(m => m.GetAllActivities())
            .Returns((GetActivitiesResult?)null);

        var activitiesServiceMock = new Mock<IActivitiesService>();
        activitiesServiceMock.Setup(x => x.GetActivitiesOfCurrentUserApiAsync(It.IsAny<DateTime>(), It.IsAny<int>()))
            .ReturnsAsync(new GetActivitiesResult(currentAndValidDays, currentAndValidActivities));

        var viewModel = new ActivitiesViewModel(new ApplicationServiceMock(), activitiesRepoMock.Object, activitiesServiceMock.Object);

        //Act
        viewModel.Init();

        //Assert
        Assert.True(viewModel.ObservableDays.ContainsKey(DateOnly.FromDateTime(DateTime.Now)));
        Assert.True(viewModel.ObservableDays.ContainsKey(DateOnly.FromDateTime(DateTime.Now.AddDays(1))));
        Assert.True(viewModel.ObservableDays.ContainsKey(DateOnly.FromDateTime(DateTime.Now.AddDays(2))));
        Assert.Equal(BaseViewModel.StateKey.Loaded, viewModel.MainStateKey);
    }
}