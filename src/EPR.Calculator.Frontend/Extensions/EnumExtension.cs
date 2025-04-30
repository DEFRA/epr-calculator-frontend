using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.Serialization;

namespace EPR.Calculator.Frontend.Extensions
{
    public static class EnumExtension
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var memberInfo = enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault();
            if (memberInfo == null)
            {
                return enumValue.ToString();
            }

            // Check for DisplayAttribute
            var displayAttribute = memberInfo.GetCustomAttribute<DisplayAttribute>();
            if (!string.IsNullOrEmpty(displayAttribute?.GetName()))
            {
                return displayAttribute.GetName()!;
            }

            // Check for DescriptionAttribute
            var descriptionAttribute = memberInfo.GetCustomAttribute<DescriptionAttribute>();
            if (!string.IsNullOrEmpty(descriptionAttribute?.Description))
            {
                return descriptionAttribute.Description!;
            }

            // Check for EnumMemberAttribute
            var enumMemberAttribute = memberInfo.GetCustomAttribute<EnumMemberAttribute>();
            if (!string.IsNullOrEmpty(enumMemberAttribute?.Value))
            {
                return enumMemberAttribute.Value!;
            }

            // Fallback to the enum name
            return enumValue.ToString();
        }
    }
}
