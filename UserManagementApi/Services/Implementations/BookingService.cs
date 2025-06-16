using Npgsql;
using UserManagementApi.Services.Interfaces;
using UserManagementApi.ViewModels;

namespace UserManagementApi.Services.Implementations;

public class BookingService : IBookingService
{
    private readonly IConfiguration _configuration;

    public BookingService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    
}
