using auctionServiceAPI.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddSingleton<MongoDBContext>();
builder.Services.AddScoped<IEffectService, EffectMongoDBService>();

// Create globally availabel HttpClient for accesing the gateway.
var gatewayUrl = builder.Configuration["GatewayUrl"] ?? "http://localhost:4000";
builder.Services.AddHttpClient("gateway", client =>
{
    client.BaseAddress = new Uri(gatewayUrl);
    client.DefaultRequestHeaders.Add(
        HeaderNames.Accept, "application/json");
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Make sure these services are registered in Program.cs



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();

var imagePath = builder.Configuration["ImagePath"];
var fileProvider = new PhysicalFileProvider(Path.GetFullPath(imagePath));
var requestPath = new PathString("/images/effect");
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = fileProvider,
    RequestPath = requestPath
});
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapRazorPages();

app.Run();
