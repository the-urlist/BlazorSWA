using System;
using System.Collections.Generic;

namespace BlazorApp.Shared;

public class User
{
  public User(ClientPrincipal clientPrincipal)
  {
    ClientPrincipal = clientPrincipal;
  }

  public bool IsLoggedIn => ClientPrincipal != null && !string.IsNullOrEmpty(ClientPrincipal.UserId);
  public ClientPrincipal ClientPrincipal { get; set; } = new ClientPrincipal();

  public List<LinkBundle> LinkBundles { get; set; } = new List<LinkBundle>();
}
