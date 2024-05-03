using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Job.Abstraction.ViewDataModels;
using Job.Abstraction.Services;
using Job.Common;
using Job.Abstraction;

namespace Job.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IAccountServices _iAccountService;
        private readonly ClaimsPrincipal _claimPincipal;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AccountController(IConfiguration config, IAccountServices iAccountService, IHttpContextAccessor httpContextAccessor)
        {
            _config = config;
            _iAccountService = iAccountService ?? throw new ArgumentNullException(nameof(iAccountService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _claimPincipal = _httpContextAccessor.HttpContext.User ?? throw new ArgumentNullException(nameof(_httpContextAccessor.HttpContext.User));
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Login(LoginDetails loginDetails)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //int a = 10, b = 0;
                    //int c = a / b;
                    LoginDetails modelUserMaster = await _iAccountService.AuthenticateUser(loginDetails);

                    if (modelUserMaster == null || (modelUserMaster != null && modelUserMaster.UserId == 0))
                    {
                        TempData["Message"] = CommonUtils.ConcatString("Invalid Username or Password", Convert.ToString((int)EnumLookup.ResponseMsgType.error), "||");
                        ViewBag.InvalidCode = true;
                        return View(modelUserMaster);
                    }
                    else
                    {
                        var claims = new List<Claim>() {
                            new Claim(ClaimTypes.NameIdentifier, modelUserMaster.UserId.ToString()),
                            new Claim(ClaimTypes.Name, modelUserMaster.DisplayName),
                            //new Claim(ClaimTypes.Role, modelUserMaster.UserTypeId.ToString()),
                            new Claim(ClaimTypes.Role,modelUserMaster.Role.ToString()),
                            new Claim(ClaimTypes.GivenName, modelUserMaster.DisplayName),
                            new Claim(ClaimTypes.Email, modelUserMaster.EmailId),
                        };


                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = false,
                            ExpiresUtc = DateTime.UtcNow.AddHours(2)
                        };
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
                        if (modelUserMaster.UserTypeId.ToString() == "1")
                        {
                            return RedirectToAction("GetJobApplicationList", "JobApplication");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return View();
        }
    }
}
