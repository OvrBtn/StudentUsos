﻿using StudentUsos.Features.Activities.Models;

namespace StudentUsos.Features.Dashboard.Views
{
    public partial class DashboardPage : ContentPage
    {
        DashboardViewModel viewModel;
        public DashboardPage(DashboardViewModel dashboardViewModel)
        {
            dashboardViewModel.PassPage(this);
            BindingContext = viewModel = dashboardViewModel;
            InitializeComponent();

            //Since user's name is just 1 label it should be fine to load it immediately
            viewModel.LoadUserName();
        }

        bool isViewModelInitialized = false;
        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            if (isViewModelInitialized)
            {
                return;
            }
            Dispatcher.Dispatch(() =>
            {
                isViewModelInitialized = true;
                _ = viewModel.InitAsync();
            });
        }

        public bool TryScrollingToItem(Activity activity)
        {
            if (carouselView == null || carouselView.ItemsSource == null) return false;
            int index = new List<Activity>(carouselView.ItemsSource.Cast<Activity>()).FindIndex(x => x.Name == activity.Name && x.CourseId == activity.CourseId);
            if (index >= 0)
            {
                carouselView.ScrollTo(index);
                activitiesIndicator.Position = carouselView.Position;
            }
            return true;
        }
    }
}