
using API.Interfaces;
using API.Repository;
using API.Services;
using UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddControllers();
// builder.Services.AddIdentity<AppUser, AppRole>(options =>
// {
//     options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
// })
    // .AddDefaultTokenProviders();
builder.Services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "wwwroot";
            });
builder.Services.AddControllersWithViews();
// builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "C Car", Version = "v1" });
});

var app = builder.Build();
app.UseDeveloperExceptionPage();
app.UseSwagger();

app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
// }

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseDefaultFiles();
app.UseStaticFiles();




using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<DataContext>();
    await context.Database.MigrateAsync();
    await Seed.SeedUsers(context);
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occured During migration");

}
await app.RunAsync();
