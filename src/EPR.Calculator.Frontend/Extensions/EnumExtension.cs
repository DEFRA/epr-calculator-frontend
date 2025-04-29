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
            if (displayAttribute != null)
            {
                return displayAttribute.GetName();
            }

            // Check for DescriptionAttribute
            var descriptionAttribute = memberInfo.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttribute != null)
            {
                return descriptionAttribute.Description;
            }

            // Check for EnumMemberAttribute
            var enumMemberAttribute = memberInfo.GetCustomAttribute<EnumMemberAttribute>();
            if (enumMemberAttribute != null)
            {
                return enumMemberAttribute.Value;
            }

            // Fallback to the enum name
            return enumValue.ToString();
        }
    }
}
