using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using NaimeiKnowledge.Models;
using NaimeiKnowledge.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ZetaLib.AspNetCore.Forum;
using ZetaLib.AspNetCore.Forum.MongoDB;
using ZetaLib.AspNetCore.Identity.MongoDB;
using ZetaLib.Database;
using ZetaLib.Database.MongoDB;
using ZetaLib.JsonApi;

namespace NaimeiKnowledge
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRepositoryProvider(options =>
            {
                options.Sources.Add(new MongoSource
                {
                    ConnectionString = this.Configuration.GetConnectionString("AWSMongoConnection"),
                    DatabaseName = "naimei"
                });
            });

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = this.Configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = this.Configuration["Authentication:Google:ClientSecret"];
                });

            services.AddIdentity<ApplicationUser, MongoIdentityRole>()
                .AddMongoStores()
                .AddDefaultTokenProviders();

            services.AddMvc();

            services.AddScoped<IForumStore<ForumPost, ForumComment, ObjectId>, MongoForumStore<ForumPost, ForumComment, ObjectId>>();
            services.AddScoped<ITagStore<ForumTag, ObjectId>, MongoTagStore<ForumTag, ObjectId>>();
            services.AddScoped<ForumManager>();
            services.AddScoped<TagManager>();

            services.AddTransient<IMailSender, MailSender>();

            services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    if (context.Response.StatusCode == 200)
                    {
                        var responseDocument = new JsonApiDocument
                        {
                            Errors = new List<JsonApiError>
                            {
                                new JsonApiError
                                {
                                    Status = StatusCodes.Status401Unauthorized.ToString(),
                                    Title = "Unauthorized",
                                    Detail = "Session invalid.",
                                }
                            },
                        };
                        var json = JsonConvert.SerializeObject(responseDocument, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, ContractResolver = new CamelCasePropertyNamesContractResolver() });
                        var data = Encoding.UTF8.GetBytes(json);
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/vnd.api+json";
                        context.Response.Body.Write(data, 0, data.Length);
                        return Task.FromResult(new { });
                    }

                    return Task.FromResult(new { redirectUri = context.RedirectUri });
                };
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
            });

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 2;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
            });

            services.Configure<MailOptions>(options =>
            {
                options.From = new MailAddress(this.Configuration["Smtp:From"], "耐美知識網", Encoding.UTF8);
                options.SmtpOptions.Credentials = new NetworkCredential
                {
                    SecurePassword = GetSecureString(this.Configuration["Smtp:Credential:Password"]),
                    UserName = this.Configuration["Smtp:Credential:UserName"],
                };
                options.SmtpOptions.Host = this.Configuration["Smtp:Host"];
                options.SmtpOptions.Port = int.Parse(this.Configuration["Smtp:Port"]);
                options.SmtpOptions.UseDefaultCredentials = false;
            });

            services.Configure<MvcJsonOptions>(options =>
            {
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });

            SecureString GetSecureString(string s)
            {
                var secureString = new SecureString();
                foreach (var c in s)
                {
                    secureString.AppendChar(c);
                }

                secureString.MakeReadOnly();
                return secureString;
            }
        }
    }
}
