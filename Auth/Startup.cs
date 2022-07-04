using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddAuthentication("Cookies").AddCookie();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                #region Menu
                endpoints.MapGet("/", async context =>
                {
                    string content = "<h1>ASP.NET Core Authorization & Authentication æ»≥Á«œººø‰</h1>";

                    content += "<a href=\"/Login\">Login</a> <br />";
                    content += "<a href=\"/Login/User\">Login(User)</a> <br />";
                    content += "<a href=\"/Login/Admin\">Login(Admin)</a> <br />";
                    content += "<a href=\"/Info\">Information</a> <br />";
                    content += "<a href=\"/InfoDetails\">Information(Details)</a> <br />";
                    content += "<a href=\"/InfoJson\">Json</a> <br />";
                    content += "<a href=\"/Logout\">Logout</a> <br />";

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

    public class ClaimDto //Data Transfer Object
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }


}
