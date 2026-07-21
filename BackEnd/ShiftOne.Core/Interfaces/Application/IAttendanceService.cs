using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.Attendance;
using ShiftOne.Shared.Responses;

namespace ShiftOne.Core.Interfaces.Application
{
    public interface IAttendanceService
    {
        Task<GeneralResponse> CheckInAsync(AttendancePunchRequest request);
        Task<GeneralResponse> CheckOutAsync(AttendancePunchRequest request);
        Task<GeneralResponse> GetMyHistoryAsync(int days = 3);
        Task<GeneralResponse> GetAllAsync(PaginationRequest request, Guid? employeeId, Guid? branchId, DateTime? from, DateTime? to);
    }
}