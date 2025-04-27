using Domain;
using Domain.Experimental;
using Domain.Models;
using Domain.Ports;

using EFDatabase;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services
    .AddDbContext<CarStorageDbContext>()
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
else
{
    app.UseExceptionHandler("/error");
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
        if (!vehicleInquiries.Any())
        {
            return Results.BadRequest();
        }

        return Results.Ok(await vehicleInquiryMatcher.Match(vehicleInquiries).ConfigureAwait(false));
    })
    .WithName("Find locations")
    .WithOpenApi();

app.Map("/error", () => Results.Problem("An error occurred."));

app.Run();
