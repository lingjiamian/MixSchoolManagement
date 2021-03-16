using EmployeeManagement.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MixSchoolManagement.Application.Dtos;
using MixSchoolManagement.EntityFrameworkCore;
using MixSchoolManagement.Infrastructure.Data;
using MixSchoolManagement.Infrastructure.Repositories;
using MixSchoolManagement.Models;
using MixSchoolManagement.Security;
using MixSchoolManagement.Security.CustomTokenProvider;
using NetCore.AutoRegisterDi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MixSchoolManagement.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration;
            WebHostEnvironment = webHostEnvironment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment WebHostEnvironment { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<AppDbContext>(optionsAction: options =>
             {
                 options.UseLazyLoadingProxies().UseSqlite(Configuration.GetConnectionString("SqliteConnStr"));
            
             });


            services.AddControllersWithViews(configure: config =>
            {
                config.Filters.Add(new AuthorizeFilter());
            }).AddRazorRuntimeCompilation().AddXmlSerializerFormatters();

            #region 健康检查 

            services.AddHealthChecks().AddSqlite(Configuration.GetConnectionString("SqliteConnStr"));

            #endregion

            #region 认证
            services.AddAuthentication()
            .AddMicrosoftAccount(microsoftOptions =>
            {
                microsoftOptions.ClientId = Configuration["Authentication:Microsoft:ClientId"];
                microsoftOptions.ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"];
            }).AddGitHub(options =>
            {
                options.ClientId = Configuration["Authentication:GitHub:ClientId"];
                options.ClientSecret = Configuration["Authentication:GitHub:ClientSecret"];
                options.Scope.Add("user:email");
            });

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 3;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.SignIn.RequireConfirmedEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            });

            services.AddIdentity<ApplicationUser, IdentityRole>()
             .AddEntityFrameworkStores<AppDbContext>()
                   .AddDefaultTokenProviders();

            #endregion


            #region 授权

            // 基于策略授权
            services.AddAuthorization(options =>
            {
                options.AddPolicy("DeleteRolePolicy",
                   policy => policy.RequireClaim("Delete Role","true"));

                options.AddPolicy("AdminRolePolicy",
                   policy => policy.RequireRole("Admin"));

                //基于策略结合多个角色进行授权
                //允许Admin, User, SuperAdmin访问
                options.AddPolicy("SuperAdminPolicy", policy =>
                  policy.RequireRole("Admin","User", "SuperManager"));

                options.AddPolicy("EditRolePolicy", policy =>
         policy.AddRequirements(new ManageAdminRolesAndClaimsRequirement()));
            });

            #endregion

            #region 依赖注入

            //获取Application类库DLL
            var assembliesToScan = new[]   {
                 Assembly.GetExecutingAssembly(),
                Assembly.GetAssembly(typeof(PagedResultDto<>))

            };
            //Assembly.GetAssembly(typeof(PagedResultDto<>)
            services.AddHttpContextAccessor();
            //自动注入服务到依赖注入容器
            services.RegisterAssemblyPublicNonGenericClasses(assembliesToScan)
             .Where(c => c.Name.EndsWith("Service"))
            .AsPublicImplementedInterfaces(ServiceLifetime.Scoped);

            services.AddSingleton<DataProtectionPurposeStrings>();

            //services.AddScoped<IStudentRepository, SQLStudentRepository>();
            //services.AddScoped<IStudentService, StudentService>();
            //services.AddScoped<ICourseService, CourseService>();

            //可编辑角色授权处理器
            services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherAdminRolesAndClaimsHandler>();

            //超级管理员授权处理器
            //services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();


            services.AddTransient(typeof(IRepository<,>), typeof(RepositoryBase<,>));
            #endregion

            #region SwaggerUI

            //Swagger文档
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "MixSchoolManagement API",
                    Description = "MixSchoolManagement系统，添加一个简单的ASP.NET Core Web API示例",
                    Version = "v1",
                    TermsOfService = new Uri("https://github.com/lingjiamian"),
                    Contact = new OpenApiContact
                    {
                        Name = "lingjiamian",
                        Email = "lingjiamian@outlook.com",
                        Url = new Uri("https://github.com/lingjiamian"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = new Uri("https://rem.mit-license.org/"),
                    }
                });
             
                if (WebHostEnvironment.IsDevelopment())
                {
                    // 设置Swagger JSON和UI的注释路径。
                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    c.IncludeXmlComments(xmlPath);
                }
            });
            #endregion

            #region 配置Cookie
            services.ConfigureApplicationCookie(configure: config =>
            {

                config.AccessDeniedPath = new Microsoft.AspNetCore.Http.PathString("/Admin/AccessDenied");

                config.Cookie.Name = "MixSchoolManagement";

                config.ExpireTimeSpan = TimeSpan.FromMinutes(180);

                //设置滑动过期
                config.SlidingExpiration = true;
            });

            #endregion

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();


            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            app.UseDataInitializer();

            app.UseSwagger();

            app.UseSwaggerUI(setupAction:options => {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "MixSchoolManagement MVC V1");
            });

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
