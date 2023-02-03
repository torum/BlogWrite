﻿using FeedDesk.Contracts.Services;
using FeedDesk.ViewModels;

using Microsoft.UI.Xaml;

namespace FeedDesk.Activation;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService _navigationService;

    public DefaultActivationHandler(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        // None of the ActivationHandlers has handled the activation.
        return _navigationService.Frame?.Content == null;
    }

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        _navigationService.NavigateTo(typeof(FeedsViewModel).FullName!, null);

        await Task.FromResult(true);
        //await Task.CompletedTask;
    }
}