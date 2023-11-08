using BlazorApp.Shared;

public class StateContainer
{
  private LinkBundle savedLinkBundle = new LinkBundle();

  public LinkBundle LinkBundle
  {
    get => savedLinkBundle ??= new LinkBundle();
    set
    {
      savedLinkBundle = value;
      NotifyStateChanged();
    }
  }

  private User user = new User();

  public User User
  {
    get => user ??= new User();
    set
    {
      user = value;
      NotifyStateChanged();
    }
  }

  public void DeleteLinkFromBundle(Link link)
  {
    LinkBundle.Links.Remove(link);
    NotifyStateChanged();
  }

  public async Task<HttpResponseMessage> GetLinkBundle(string vanityUrl)
  {
    var client = new HttpClient();
    var response = await client.GetAsync($"links/{vanityUrl}");
    return response;
  }

  public event Action? OnChange;

  private void NotifyStateChanged() => OnChange?.Invoke();
}