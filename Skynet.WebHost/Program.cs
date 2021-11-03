using Skynet.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add all the json files found under Data/Configs
foreach(var file in Directory.GetFiles(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Data", "Configs"), "*.json"))
{
    builder.Configuration.AddJsonFile(file);
}

// This is using Assembly Scanning - so the assembly with Program will be used
// to retrieve user-secrets and add them
builder.Configuration.AddUserSecrets<Program>();

// Add the various skynet services required for our application
builder.Services.AddSkynetServices(builder.Configuration);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
