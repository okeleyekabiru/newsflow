using Amazon.S3;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NewsFlow.API.Behaviours;
using NewsFlow.API.Hubs;
using NewsFlow.Core.Common;
using NewsFlow.Core.Entities;
using NewsFlow.Core.Interfaces;
using NewsFlow.Infrastructure.ContentFilters;
using NewsFlow.Infrastructure.Data;
using NewsFlow.Infrastructure.ExternalServices;
using NewsFlow.Infrastructure.Pipeline;
using NewsFlow.Infrastructure.Repositories;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddDbContext<NewsFlowDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

// Microsoft Identity — use AddIdentityCore so cookie auth is NOT registered as the
// default scheme (which would conflict with JwtBearer).  SignInManager and
// DefaultTokenProviders are added explicitly to get the full Identity feature set.
builder.Services.AddIdentityCore<User>(opts =>
{
    opts.Password.RequireDigit = true;
    opts.Password.RequiredLength = 8;
    opts.Password.RequireNonAlphanumeric = false;
    opts.Password.RequireUppercase = false;
    opts.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole<Guid>>()
.AddEntityFrameworkStores<NewsFlowDbContext>()
.AddDefaultTokenProviders()
.AddSignInManager<SignInManager<User>>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
        opts.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) &&
                    ctx.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    ctx.Token = token;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
});

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<IContentFilterStrategy, ConflictFilterStrategy>();
builder.Services.AddScoped<IContentFilterStrategy, TerrorismFilterStrategy>();
builder.Services.AddScoped<IContentFilterStrategy, DefaultFilterStrategy>();
builder.Services.AddScoped<IContentFilterContext, ContentFilterContext>();

builder.Services.AddScoped<IPlatformAdapter, TikTokAdapter>();
builder.Services.AddScoped<IPlatformAdapter, TwitterAdapter>();
builder.Services.AddScoped<IPlatformAdapter, InstagramAdapter>();
builder.Services.AddScoped<IPlatformAdapter, YouTubeAdapter>();
builder.Services.AddScoped<IPlatformAdapterFactory, PlatformAdapterFactory>();

builder.Services.AddHttpClient<IAIProvider, ClaudeAIProvider>();
builder.Services.AddHttpClient<IVoiceProvider, ElevenLabsVoiceProvider>();
builder.Services.AddHttpClient<IStockFootageProvider, PexelsFootageProvider>();

builder.Services.AddSingleton<IVideoAssembler, FfmpegVideoAssembler>();

// AWS S3 — credentials resolved from env vars / ~/.aws / IAM role automatically
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddScoped<IStorageService, S3StorageService>();

builder.Services.AddScoped<IngestPipelineFactory>();
builder.Services.AddScoped<PostBuilder>();

builder.Services.AddScoped<VideoGeneratorFactory>();
builder.Services.AddScoped<TikTokVideoGenerator>();
builder.Services.AddScoped<YouTubeVideoGenerator>();
builder.Services.AddScoped<ReelsVideoGenerator>();

builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(p =>
        p.WithOrigins(builder.Configuration["Cors:AllowedOrigins"]?.Split(',') ?? ["http://localhost:3000"])
         .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

builder.Services.AddStackExchangeRedisCache(opts =>
    opts.Configuration = builder.Configuration.GetConnectionString("Redis"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<CollaborationHub>("/hubs/collaboration");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NewsFlowDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
