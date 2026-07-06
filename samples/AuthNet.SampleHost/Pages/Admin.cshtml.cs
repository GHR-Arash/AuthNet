using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthNet.SampleHost.Pages;

[Authorize(Roles = "Administrator")]
public sealed class AdminModel : PageModel;

