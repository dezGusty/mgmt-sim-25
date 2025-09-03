using ManagementSimulator.Core.Dtos.Responses.User;

namespace ManagementSimulator.Core.Dtos.Responses
{
    public class SecondManagerResponseDto
    {
        public int SecondManagerEmployeeId { get; set; }
        public string SecondManagerEmployeeName { get; set; } = string.Empty;
        public string SecondManagerEmployeeEmail { get; set; } = string.Empty;

        public int ReplacedManagerId { get; set; }
        public string ReplacedManagerName { get; set; } = string.Empty;
        public string ReplacedManagerEmail { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
} 