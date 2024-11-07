using Common.Enums;
using System.ComponentModel.DataAnnotations;
using TaskManagementApi.Extensions;

namespace TaskManagementApi.Models
{
    public class CreateTaskArgs
    {
        [Required]
        public required string Name { get; set; }

        [Required]
        public required string Description { get; set; }

        [Required]
        [EnumValidation(typeof(Status))]
        public required Status Status { get; set; }
        public string? AssignedTo { get; set; }
    }
}
