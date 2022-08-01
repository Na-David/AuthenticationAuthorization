using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace Auth
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddAuthentication("Cookies").AddCookie();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

            //services.AddMvc();
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //MiddleWare
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization(); // To use Authorize

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute(); // To use MVC, Need to be added

                #region Menu
                endpoints.MapGet("/", async context =>
                {
                    string content = "<h1>ASP.NET Core Authorization & Authentication æ»≥Á«œººø‰</h1>";

                    #region LinkList
                    content += "<a href=\"/Login\">Login</a> <br />";
                    content += "<a href=\"/Login/User\">Login(User)</a> <br />";
                    content += "<a href=\"/Login/Admin\">Login(Admin)</a> <br />";
                    content += "<a href=\"/Info\">Information</a> <br />";
                    content += "<a href=\"/InfoDetails\">Information(Details)</a> <br />";
                    content += "<a href=\"/InfoJson\">Json</a> <br />";
                    content += "<a href=\"/Logout\">Logout</a> <br />";
                    content += "<hr><a href=\"/Landing/Index\">Landing Page</a> <br />";
                    content += "<a href=\"/Greeting\">Greeting</a> <br />";
                    content += "<a href=\"/Dashboard\">Dashboard</a> <br />";
                    content += "<hr><a href=\"/api/AuthService\">Login Info(Json)</a> <br />";

                    #endregion
                    context.Response.Headers["Content-Type"] = "text/html; charset = utf-8";
                    await context.Response.WriteAsync(content);
                });

                #endregion

                #region Login
                endpoints.MapGet("/Login", async context =>
                {
                    var claims = new List<Claim>
                    {
                                new Claim(ClaimTypes.Name, "Aaron")
                    };
                            //var claimsIdentity = new ClaimsIdentity(claims, "cookies");
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                    context.Response.Headers["Content-Type"] = "text/html; charset = utf-8";
                    await context.Response.WriteAsync("<h3>Login Successful</h3>");
                });
                #endregion

                #region /Login/{Username}
                endpoints.MapGet("/Login/{Username}", async context =>
                {
                    var username = context.Request.RouteValues["Username"].ToString(); // taking the token value into username
                    var claims = new List<Claim>
                    {
                                new Claim(ClaimTypes.Name, username),
                                new Claim(ClaimTypes.NameIdentifier, username),
                                new Claim(ClaimTypes.Email, username + "@naver.com"),
                                new Claim(ClaimTypes.Role, "User"),
                                new Claim("Any name", "Any Value")
                    };

                    if (username == "Admin")
                    {
                        claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                    }
                            //var claimsIdentity = new ClaimsIdentity(claims, "cookies");
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await context.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        claimsPrincipal,
                        new AuthenticationProperties { IsPersistent = true }); // when we close web browser, this cookie still exists. if the value is false, then that cookie will be dissappear.

                    context.Response.Headers["Content-Type"] = "text/html; charset = utf-8";
                    await context.Response.WriteAsync("<h3>Login Successful</h3>");
                });
                #endregion

                #region InfoDetails
                endpoints.MapGet("/InfoDetails", async context =>
                {
                    string result = "";

                    if (context.User.Identity.IsAuthenticated)
                    {
                        result += $"<h3>Login Name : {context.User.Identity.Name}</h3>";
                        foreach (var claim in context.User.Claims)
                        {
                            result += $"{claim.Type} = {claim.Value} <br />";
                        }
                        if (context.User.IsInRole("Admin") && context.User.IsInRole("User"))//if this token has those two conditions,
                        {
                            result += "<br />Authorization details : Administrator + User";
                        }
                    }
                    else
                    {
                        result += "Login Failed";
                    }

                    context.Response.Headers["Content-Type"] = "text/html; charset = utf-8";
                    await context.Response.WriteAsync(result, Encoding.Default);

                }); 
                #endregion

                #region Info
                endpoints.MapGet("/Info", async context =>
                {
                    string result = "";

                    if (context.User.Identity.IsAuthenticated)
                    {
                        result += $"<h3>Login Name : {context.User.Identity.Name}</h3>";
                    }
                    else
                    {
                        result += "Login Failed";
                    }

                    context.Response.Headers["Content-Type"] = "text/html; charset = utf-8";
                    await context.Response.WriteAsync(result, Encoding.Default);

                });
                #endregion

                #region InfoJson
                endpoints.MapGet("/InfoJson", async context =>
                {
                    string json = "";

                    if (context.User.Identity.IsAuthenticated)
                    {
                        //json += "{ \"Type\" : \"Name\", \"Value\" : \"User Name\"}";
                        var claims = context.User.Claims.Select(c => new ClaimDto { Type = c.Type, Value = c.Value});
                        json += JsonSerializer.Serialize<IEnumerable<ClaimDto>>(claims, new JsonSerializerOptions {Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
                    }
                    else
                    {
                        json += "{ }";
                    }
                    //text/html //// application/json == mime type
                    context.Response.Headers["Content-Type"] = "application/json; charset = utf-8";
                    await context.Response.WriteAsync(json);

                });
                #endregion

                #region Logout
                endpoints.MapGet("/Logout", async context =>
                {
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                    context.Response.Headers["Content-Type"] = "text/html; charset = utf-8";
                    await context.Response.WriteAsync("<h3>Successfulyl Logged out</h3>");
                }); 
                #endregion
            });
        }
    }

    #region Data Transfer Object
    public class ClaimDto //Data Transfer Object
    {
        public string Type { get; set; }
        public string Value { get; set; }
    } 
    #endregion

    #region MVC Controller
    //MVC Model
    [AllowAnonymous]
    public class LandingController : Controller
    {
        public IActionResult Index() => Content("Anyone can get an access");

        [Authorize]//Greeting page will not open to anyone. Unless the user is logged on
        [Route("/Greeting")]
        public IActionResult Greeting()
        {
            var roleName = HttpContext.User.IsInRole("Admin") ? "Administrator" : "User";
            return Content($"Hello, <em>{roleName}</em>", "text/html", Encoding.Default);
        }
    }

    [Authorize(Roles = "Admin")] // Onle Admin Role can get an access
    public class DashboardController : Controller
    {
        public IActionResult Index() => Content("Hello, Admin.");
    }
    #endregion

    #region Web API Controller
    [ApiController]
    [Route("api/[controller]")]
    public class AuthServiceController : ControllerBase
    {
        [Authorize]
        [HttpGet]
        public IEnumerable<ClaimDto> Get() =>
            HttpContext.User.Claims.Select(c => new ClaimDto { Type = c.Type, Value = c.Value });
    } 
    #endregion
}
