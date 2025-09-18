using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagementSimulator.Database.Entities
{
    public class AuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Action information
        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE, LOGIN, LOGOUT, etc.

        [Required]
        [MaxLength(100)]
        public string EntityType { get; set; } = string.Empty; // User, Project, Department, etc.

        public int? EntityId { get; set; } // ID of the affected entity

        [MaxLength(500)]
        public string EntityName { get; set; } = string.Empty; // Human readable name of the entity

        // User context information
        [Required]
        public int UserId { get; set; } // User who performed the action

        [Required]
        [MaxLength(100)]
        public string UserEmail { get; set; } = string.Empty;

        [MaxLength(200)]
        public string UserRoles { get; set; } = string.Empty; // Comma-separated roles

        // Impersonation context
        public bool IsImpersonating { get; set; } = false;
        public int? OriginalUserId { get; set; } // Admin who is impersonating
        [MaxLength(100)]
        public string? OriginalUserEmail { get; set; }

        // Request context
        [Required]
        [MaxLength(10)]
        public string HttpMethod { get; set; } = string.Empty; // GET, POST, PUT, DELETE

        [Required]
        [MaxLength(500)]
        public string Endpoint { get; set; } = string.Empty; // /api/users/123

        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        // Data changes (JSON format for flexibility)
        public string? OldValues { get; set; } // JSON of previous state
        public string? NewValues { get; set; } // JSON of new state
        public string? AdditionalData { get; set; } // JSON for extra context

        // Timing
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Result information
        public bool Success { get; set; } = true;
        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        // Optional description for human readability
        [MaxLength(1000)]
        public string? Description { get; set; }

        // Navigation properties (if needed)
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        [ForeignKey(nameof(OriginalUserId))]
        public virtual User? OriginalUser { get; set; }
    }
}