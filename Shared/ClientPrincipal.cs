using System.Collections.Generic;

namespace BlazorApp.Shared;

public class ClientPrincipal
{
  public string IdentityProvider { get; set; } = "";
  public string UserId { get; set; } = "";
  public string UserDetails { get; set; } = "";
  public IEnumerable<string> UserRoles { get; set; }
}