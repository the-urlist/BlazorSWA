using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace BlazorApp.Shared;

public class User
{
  public User(ClientPrincipal clientPrincipal)
  {
    ClientPrincipal = clientPrincipal;

    // depending on the provider, the username and image may be in a different place in the
    var identity = new ClaimsIdentity(clientPrincipal.IdentityProvider);

    // google
    if (clientPrincipal.IdentityProvider == "google")
    {
      Console.WriteLine(clientPrincipal.Claims.Count);
      UserName = clientPrincipal.Claims.Find(c => c.Type == "name")?.Value;
      UserImage = clientPrincipal.Claims.Find(c => c.Type == "picture")?.Value;
    }

    // github
    if (clientPrincipal.IdentityProvider == "github")
    {
      Console.WriteLine(clientPrincipal.Claims.Count);
      UserName = clientPrincipal.Claims.Find(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;
      UserImage = clientPrincipal.Claims.Find(c => c.Type == "urn:github:avatar_url")?.Value;
    }

    // // facebook
    // if (clientPrincipal.IdentityProvider == "facebook")
    // {
    //   UserName = identity.FindFirst("name")?.Value;
    //   UserImage = identity.FindFirst("picture")?.Value;
    // }
  }

  public string UserName { get; set; }
  public string UserImage { get; set; }

  public bool IsLoggedIn => ClientPrincipal != null && !string.IsNullOrEmpty(ClientPrincipal.UserId);
  public ClientPrincipal ClientPrincipal { get; set; } = new ClientPrincipal();

  public List<LinkBundle> LinkBundles { get; set; } = new List<LinkBundle>();
}
