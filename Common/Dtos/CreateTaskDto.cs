using Common.Enums;

namespace Common.Dtos
{
    public class CreateTaskDto
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required Status Status { get; set; }
        public string? AssignedTo { get; set; }
    }
}
