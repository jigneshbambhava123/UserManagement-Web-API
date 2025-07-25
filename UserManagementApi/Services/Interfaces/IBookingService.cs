using UserManagementApi.ViewModels;

namespace UserManagementApi.Services.Interfaces;

public interface IBookingService
{
    Task CreateBooking(BookingViewModel booking);

    Task UpdateToDate(int bookingId, DateTime toDate);

    Task<(List<BookingViewModel> Bookings, int TotalCount)> GetBookingHistoryFilteredAsync(
        int? userId = null,
        string? search = null,
        string? sortColumn = "todate",
        string? sortDirection = "desc",
        int pageNumber = 1,
        int pageSize = 10,
        string? timeFilter = null 
    );

    Task<(List<BookingViewModel> Bookings, int TotalCount)> GetActiveBookingsFilteredAsync(
        int? userId = null,
        string? search = null,
        string? sortColumn = "todate",
        string? sortDirection = "desc",
        int pageNumber = 1,
        int pageSize = 10,
        string? timeFilter = null
    );

    Task ReleaseExpiredBookings();
}
