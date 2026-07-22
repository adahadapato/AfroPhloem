using AfroPhloem.Models;

namespace AfroPhloem.Services;

public class SessionService
{
    public User? CurrentUser { get; set; }

    public bool IsLoggedIn => CurrentUser is not null;

    public bool IsAdmin => CurrentUser?.IsAdmin ?? false;

    public void SignOut() => CurrentUser = null;
}