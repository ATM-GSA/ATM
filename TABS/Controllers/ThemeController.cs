using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Route("[controller]/[action]")]
public class ThemeController : Controller
{
    public async Task<LocalRedirectResult> Set(string theme, string redirectUri)
    {
        // function serves as a means to reload the page
        return LocalRedirect(redirectUri);
    }
}
