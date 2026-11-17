namespace DemoMVC.ViewModels;

public class ManagedUserViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Roles { get; set; } = string.Empty;
    public string SelectedRole { get; set; } = "User";
    public bool IsAdmin { get; set; }
    public bool IsCurrentUser { get; set; }
    public bool IsLockedOut { get; set; }
}
