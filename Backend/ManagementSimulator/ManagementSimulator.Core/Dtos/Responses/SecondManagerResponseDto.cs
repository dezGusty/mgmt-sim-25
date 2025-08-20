using ManagementSimulator.Core.Dtos.Responses.User;

namespace ManagementSimulator.Core.Dtos.Responses
{
    public class SecondManagerResponseDto
    {
        public int SecondManagerEmployeeId { get; set; }
        public UserResponseDto SecondManagerEmployee { get; set; }
        
        public int ReplacedManagerId { get; set; }
        public UserResponseDto ReplacedManager { get; set; }
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
} 