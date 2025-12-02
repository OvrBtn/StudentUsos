using StudentUsos.Features.Authorization.Models;
using StudentUsos.Features.Authorization.Services;

namespace StudentUsos.Features.Authorization.Views;

public record LoginResultParameters(UsosInstallation Installation, AuthorizationService.Mode DefaultMode = AuthorizationService.Mode.RedirectWithCallback);
