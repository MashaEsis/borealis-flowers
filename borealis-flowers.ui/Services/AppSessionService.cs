using System.Net.Http.Json;
using System.Text.Json;
using borealis_flowers.ui.Models;

namespace borealis_flowers.ui.Services;

public sealed class AppSessionService(JwtState jwt)
{
    static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    UserMeDto? _user;

    public event Action? Changed;

    public Guid ClientProfileId => _user?.Id ?? Guid.Empty;

    public Guid ActingFloristId => _user?.SpecialistId ?? Guid.Empty;

    public string DisplayName => _user?.Name ?? "Посетитель";

    public UserMeDto? CurrentUser => _user;

    public string? ApiRole => _user?.Role;

    public StudioRole Role
    {
        get
        {
            if (_user is null || string.IsNullOrEmpty(jwt.BearerToken))
                return StudioRole.Visitor;
            return MapRole(_user.Role);
        }
    }

    public bool IsVisitor => Role == StudioRole.Visitor;

    public bool IsClient => Role == StudioRole.Client;

    public bool IsFlorist => Role == StudioRole.Florist;

    public bool IsAdmin => Role == StudioRole.Admin;

    static StudioRole MapRole(string apiRole) =>
        apiRole switch
        {
            "Admin" => StudioRole.Admin,
            "Florist" => StudioRole.Florist,
            "Client" => StudioRole.Client,
            _ => StudioRole.Visitor,
        };

    public async Task ApplyAuthAsync(AuthResponseDto response, BrowserStorageService browser)
    {
        _user = response.User;
        jwt.BearerToken = response.Token;
        await browser.SetRawAsync(KJwt, response.Token);
        await browser.SetRawAsync(KUser, JsonSerializer.Serialize(response.User, JsonOpts));
        Notify();
    }

    public async Task RestoreAsync(BrowserStorageService browser)
    {
        string? token = await browser.GetRawAsync(KJwt);
        string? userJson = await browser.GetRawAsync(KUser);
        if (string.IsNullOrEmpty(token))
        {
            ClearLocal();
            Notify();
            return;
        }

        jwt.BearerToken = token;
        if (!string.IsNullOrEmpty(userJson))
            _user = JsonSerializer.Deserialize<UserMeDto>(userJson, JsonOpts);

        Notify();
    }

    public async Task SignOutAsync(BrowserStorageService browser)
    {
        await browser.RemoveAsync(KJwt);
        await browser.RemoveAsync(KUser);
        ClearLocal();
        Notify();
    }

    public async Task RefreshMeAsync(BackendApiClient api, BrowserStorageService browser)
    {
        try
        {
            UserMeDto? me = await api.Http.GetFromJsonAsync<UserMeDto>("auth/me", JsonOpts);
            if (me is null)
                return;
            _user = me;
            await browser.SetRawAsync(KUser, JsonSerializer.Serialize(me, JsonOpts));
            Notify();
        }
        catch
        {
            // сеть / 401 — оставляем кэш пользователя из localStorage
        }
    }

    void ClearLocal()
    {
        jwt.BearerToken = null;
        _user = null;
    }

    void Notify() => Changed?.Invoke();

    const string KJwt = "borealis-jwt";

    const string KUser = "borealis-user";
}
