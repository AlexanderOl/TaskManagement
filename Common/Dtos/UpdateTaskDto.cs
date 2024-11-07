using Common.Enums;

namespace Common.Dtos
{
    public class UpdateTaskDto
    {
        public int Id { get; set; }
        public Status NewStatus { get; set; }
    }
}
