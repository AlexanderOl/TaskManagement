using Common.Enums;
using TaskManagementApi.Extensions;

namespace TaskManagementApi.Models
{
    public class UpdateTaskArgs
    {
        public int Id { get; set; }
        [EnumValidation(typeof(Status))]
        public Status NewStatus { get; set; }
    }
}
