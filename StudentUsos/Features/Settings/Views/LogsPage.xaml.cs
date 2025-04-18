﻿using StudentUsos.Controls;

namespace StudentUsos.Features.Settings.Views;

public partial class LogsPage : CustomContentPageNotAnimated
{
    LogsViewModel viewModel;
    public LogsPage(LogsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
    }

    bool isViewModelSet = false;
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (isViewModelSet)
        {
            return;
        }
        isViewModelSet = true;
        Dispatcher.Dispatch(() =>
        {
            viewModel.Init();
        });
    }
}