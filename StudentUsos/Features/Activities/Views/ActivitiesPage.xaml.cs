using CustomSchedule;
using StudentUsos.Controls;
using StudentUsos.Features.Person.Models;

namespace StudentUsos.Features.Activities.Views;

public partial class ActivitiesPage : CustomContentPageNotAnimated, IQueryAttributable
{
    ActivitiesViewModel activitiesViewModel;
    public ActivitiesPage(ActivitiesViewModel activitiesViewModel)
    {
        InitializeComponent();
        BindingContext = this.activitiesViewModel = activitiesViewModel;

        activitiesViewModel.DateChanged += ChangeTodayButtonVisibility;
        //for some reason binding doesn't work with this command so it has to be assigned manually
        LoadMoreButtonElement.Command = activitiesViewModel.LoadMoreCommand;
    }

    public ScrollView TimetableScrollViewReference;

    //this will get executed when this page is created by Shell
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        Init(null);
    }

    public Schedule Schedule { get; set; }
    Lecturer? lecturer;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="lecturer">default value is null meaning a timetable of currently logged in user will be displayed, 
    /// other values will create page containing staff timetable using services/tt/staff method</param>
    public void Init(Lecturer? lecturer = null)
    {
        this.lecturer = lecturer;

        //schedule using XAML was replaced with CustomSchedule package so timetableScrollViewReference can point to empty object
        //(it's not completely removed in case MAUI's performance improves)
        //timetableScrollViewReference = timetableScrollView;
        TimetableScrollViewReference = new ScrollView();

        Schedule = schedule;
    }

    bool isViewModelSet = false;
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (isViewModelSet == false)
        {
            Dispatcher.Dispatch(() =>
            {
                isViewModelSet = true;
                if (lecturer == null)
                {
                    activitiesViewModel.Init();
                }
                else
                {
                    activitiesViewModel.Init(lecturer);
                }
            });
        }

    }

    void ChangeTodayButtonVisibility()
    {
        bool isTodaySelected = activitiesViewModel.DateOnlyPicked == DateOnly.FromDateTime(DateTimeOffset.Now.DateTime);
        if (isTodaySelected == false && (int)Math.Round(TodayButton.Opacity) == 1)
        {
            return;
        }
        if (isTodaySelected == false)
        {
            TodayButton.IsVisible = true;
        }
        Helpers.Utilities.Animate(this, (progress) =>
        {
            if (isTodaySelected)
            {
                TodayButton.Opacity = 1 - (float)progress;
            }
            else
            {
                TodayButton.Opacity = (float)progress;
            }
        }, 200, Easing.Linear, 16, () => false, (d, b) =>
        {
            if (isTodaySelected)
            {
                TodayButton.IsVisible = false;
            }
        });
    }
}