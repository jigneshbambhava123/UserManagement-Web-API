// In your UserManagementApi project's Program.cs

using UserManagementApi.Services;

var builder = WebApplication.CreateBuilder(args);

// --- START CORS CONFIGURATION ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", 
        corsBuilder => corsBuilder.WithOrigins("http://localhost:5272") 
                                  .AllowAnyHeader()
                                  .AllowAnyMethod());
});
// --- END CORS CONFIGURATION ---

builder.Services.AddControllers();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- CORS Middleware Usage ---
app.UseRouting(); 
app.UseCors("AllowSpecificOrigin"); 


app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();