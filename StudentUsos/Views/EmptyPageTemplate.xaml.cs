﻿using StudentUsos.Controls;

namespace StudentUsos;

public partial class EmptyPageTemplate : CustomContentPageNotAnimated
{
    public EmptyPageTemplate()
    {
        InitializeComponent();
    }

    //bool isViewModelSet = false;
    //protected override void OnNavigatedTo(NavigatedToEventArgs args)
    //{
    //    base.OnNavigatedTo(args);

    //    if (isViewModelSet)
    //    {
    //        return;
    //    }
    //    Dispatcher.Dispatch(() =>
    //    {
    //        isViewModelSet = true;
    //        viewModel.Init();
    //    });
    //}
}