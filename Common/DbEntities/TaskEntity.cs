using Common.Enums;

namespace Common.DbEntities
{
    public class TaskEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required Status Status { get; set; }
        public string? AssignedTo { get; set; }
    }
}
