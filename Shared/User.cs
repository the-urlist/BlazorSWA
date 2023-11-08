namespace BlazorApp.Shared
{
  public class User
  {
    public bool LoggedIn { get; set; } = false;
    public ClientPrincipal ClientPrincipal { get; set; } = new ClientPrincipal();
  }
}