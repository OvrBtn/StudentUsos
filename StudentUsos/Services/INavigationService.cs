﻿namespace StudentUsos.Services;

public interface INavigationService
{
    public Task SetAsRootPageAsync(string location, bool isAnimated = true);
    public Task<T> PushAsync<T>(bool isAnimated = true) where T : ContentPage;
    public Task<T> PushModalAsync<T>(bool isAnimated = true) where T : ContentPage;
    public Task PopAsync(bool isAnimated = true);
    public Task PopModalAsync(bool isAnimated = true);
    public Task PopToRootAsync(bool isAnimated = true);
}