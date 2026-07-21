using System.ComponentModel.DataAnnotations;

namespace ShiftOne.Shared.Requests.Devices
{
    public class ResetEmployeeDeviceRequest
    {
        [Required] public Guid EmployeeId { get; set; }
    }
}
