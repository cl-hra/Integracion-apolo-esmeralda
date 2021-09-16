using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using WebService.Services;

namespace WebService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddControllers();
            services.AddDbContext<EsmeraldaContext>(
                options => options.AddMysqlDb(Configuration)
            );
            services.AddAuthentication(
                         o =>
                         {
                             o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                             o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                         }
                     )
                    .AddJwtBearer(
                         options =>
                             options.TokenValidationParameters = new TokenValidationParameters
                             {
                                 ValidateIssuer = true,
                                 ValidateAudience = true,
                                 ValidateLifetime = true,
                                 ValidateIssuerSigningKey = true,
                                 ValidIssuer = "apolosalud.net",
                                 ValidAudience = "apolosalud.net",
                                 IssuerSigningKey = new SymmetricSecurityKey(
                                     Encoding.UTF8.GetBytes(Configuration["llavePrincipal"])
                                 ),
                                 ClockSkew = TimeSpan.Zero
                             }
                     );

            services.AddHealthChecks()
                    .AddCheck("self", () => HealthCheckResult.Healthy())
                    .AddDbContextCheck<EsmeraldaContext>(
                         tags: new[] { "service" },
                         customTestQuery: async (context, token) =>
                         {
                             var database = context.Database;
                             return await database.CanConnectAsync();
                         }
                     );

            services.AddControllers(
                options =>
                {
                    options.Filters.Add<LogActionFilter>();
                });

            services.AddRouting(o => o.LowercaseUrls = true);

            services.AddCors(
                options =>
                {
                    options.AddPolicy(
                        "CorsPolicy",
                        b => b
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowAnyOrigin()
                    );
                }
            );

            services.AddSwaggerGen(
                c =>
                {
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        BearerFormat = "JWT",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        Description = "Bearer JWT"
                    });
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[]{}
                        }
                    });
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Monitor Esmeralda Api", Version = "v1" });
                    var docXml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var pathHelp = Path.Combine(AppContext.BaseDirectory, docXml);
                    c.IncludeXmlComments(pathHelp);
                }
            );

            services.AddHttpClient("conexionApiMinsal", ConfigureClient);
            services.AddHttpClient("conexionEsmeralda", ConfigureClientEme);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseMiddleware<SerilogBodyRequestLogger>();
            
            app.UseSerilogRequestLogging(
                options =>
                {
                    options.EnrichDiagnosticContext = EnrichDiagnosticContext;
                    options.GetLevel = ExcludeHealthChecks;
                } );
            
            app.UseSwagger(c => c.RouteTemplate = "apolohra/apidocs/{documentname}/docs.json");
            app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/apolohra/apidocs/v1/docs.json", "Monitor Esmeralda Api");
                    c.RoutePrefix = "apolohra/apidocs";
                }
            );
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseAuthorization();

            app.UseEndpoints(
                endpoints =>
                {
                    endpoints.MapHealthChecks(
                        "/self",
                        new HealthCheckOptions {Predicate = r => r.Name.Contains("self")}
                    );

                    endpoints.MapHealthChecks(
                        "/ready",
                        new HealthCheckOptions {Predicate = r => r.Tags.Contains("service")}
                    );
                    
                    endpoints.MapControllers();
                } );
        }

        private LogEventLevel ExcludeHealthChecks(HttpContext httpContext, double _, Exception ex)
        {
            return ex != null
                ? LogEventLevel.Error 
                : httpContext.Response.StatusCode > 499 
                    ? LogEventLevel.Error 
                    : IsHealthCheckEndpoint(httpContext) // Not an error, check if it was a health check
                        ? LogEventLevel.Verbose // Was a health check, use Verbose
                        : LogEventLevel.Information;
        }

        private bool IsHealthCheckEndpoint(HttpContext httpContext)
        {
            var endpoint = httpContext.GetEndpoint();
            if (endpoint is object) // same as !(endpoint is null)
            {
                return string.Equals(
                    endpoint.DisplayName, 
                    "Health checks",
                    StringComparison.Ordinal);
            }
            // No endpoint, so not a health check endpoint
            return false;
        }

        private void EnrichDiagnosticContext(IDiagnosticContext diagnosticContext, HttpContext httpContext)
        {
            var request = httpContext.Request;

            diagnosticContext.Set("Host", request.Host);
            diagnosticContext.Set("Protocol", request.Protocol);
            diagnosticContext.Set("Scheme", request.Scheme);

            if(request.QueryString.HasValue)
            {
                diagnosticContext.Set("QueryString", request.QueryString.Value);
            }

            diagnosticContext.Set("ContentType", httpContext.Response.ContentType);
            
            var endpoint = httpContext.GetEndpoint();
            if (endpoint is object) // endpoint != null
            {
                diagnosticContext.Set("EndpointName", endpoint.DisplayName);
            }            
        }

        //metodo para conexion minsal
        private void ConfigureClient(HttpClient client)
        {
            client.BaseAddress = new Uri(Configuration["urlApiMinsal"]);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "HttpClient");
        }

        private void ConfigureClientEme(HttpClient client)
        {
            client.BaseAddress = new Uri(Configuration["urlEsmeralda"]);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "HttpClient");
        }
    }
}
