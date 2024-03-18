using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorApp.Shared;

public class ClientPrincipal
{
  [JsonPropertyName("userId")]
  public string UserId { get; set; }

  [JsonPropertyName("userRoles")]
  public IEnumerable<string> UserRoles { get; set; }

  [JsonPropertyName("identityProvider")]
  public string IdentityProvider { get; set; }

  [JsonPropertyName("userDetails")]
  public string UserDetails { get; set; }

  public List<Claim> Claims { get; set; } = new List<Claim>();
}

