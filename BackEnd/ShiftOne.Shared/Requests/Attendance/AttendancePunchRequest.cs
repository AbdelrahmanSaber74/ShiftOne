using System.ComponentModel.DataAnnotations;

namespace ShiftOne.Shared.Requests.Attendance
{
    public class AttendancePunchRequest
    {
        [Required] public string DeviceId { get; set; } = string.Empty;
        [Range(-90, 90)] public decimal Latitude { get; set; }
        [Range(-180, 180)] public decimal Longitude { get; set; }
    }
}
