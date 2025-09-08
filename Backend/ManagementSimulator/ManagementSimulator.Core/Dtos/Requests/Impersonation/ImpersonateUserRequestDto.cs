using System.ComponentModel.DataAnnotations;

namespace ManagementSimulator.Core.Dtos.Requests.Impersonation
{
    public class ImpersonateUserRequestDto
    {
        [Required]
        public int UserId { get; set; }
    }
}