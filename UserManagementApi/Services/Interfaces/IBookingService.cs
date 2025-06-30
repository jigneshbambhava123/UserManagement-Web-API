using UserManagementApi.ViewModels;

namespace UserManagementApi.Services.Interfaces;

public interface IBookingService
{
    Task CreateBooking(BookingViewModel booking);

    Task<List<BookingViewModel>> GetBookingHistory(int? userId = null);

    Task<List<BookingViewModel>> GetActiveBookings(int? userId = null);

    Task ReleaseExpiredBookings();
}
