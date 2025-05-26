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

// Configure CORS policy to allow your frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:8080")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

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
// CORS middleware must be early in the pipeline
app.UseCors("AllowFrontend");
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
