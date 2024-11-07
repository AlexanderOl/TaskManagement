using System.ComponentModel.DataAnnotations;

namespace TaskManagementApi.Extensions
{
    public class EnumValidationAttribute(Type enumType) : ValidationAttribute
    {
        public override bool IsValid(object? value) => Enum.IsDefined(enumType, value!);
    }
}
