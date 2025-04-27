using Domain;
using Domain.DatabasePorts;

using EFDatabase;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services
    .AddDbContext<CarStorageDbContext>()
    .AddScoped<IListingRepository, ListingRepository>()
    .AddScoped<ILocationRepository, LocationRepository>()
    .AddScoped<IVehicleInquiryMatcher, VehicleInquiryMatcherGreedy>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<CarStorageDbContext>();

    context.Database.EnsureCreated();
}

app.MapPost("/", async (List<VehicleInquiry> vehicleInquiries, IVehicleInquiryMatcher vehicleInquiryMatcher) =>
    {
        return await vehicleInquiryMatcher.Match(vehicleInquiries).ConfigureAwait(false);
    })
    .WithName("Find locations")
    .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
