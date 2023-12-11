using ChocarService;
using ChocarService.Data;
using ChocarService.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to IOC.
builder.Services.AddControllers();
builder.Services.AddDbContext<ChocarDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["IdentityServiceUrl"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.NameClaimType = "username";
    });
builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        b =>
        {
            b.AllowAnyHeader()
            .AllowAnyMethod().AllowCredentials().WithOrigins(builder.Configuration["ClientApp"]);
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowAll");
app.MapControllers();
app.MapHub<NotificationHub>("/notifications");

try
{
    DbInitializer.InitDb(app);
}
catch (Exception e)
{
    Console.WriteLine(e);
}

app.Run();
