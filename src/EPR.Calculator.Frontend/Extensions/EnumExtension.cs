using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;

namespace EPR.Calculator.Frontend.Extensions
{
    /// <summary>
    /// Extension methods for Enum types.
    /// </summary>
    public static class EnumExtension
    {
        /// <summary>
        /// Retrieves a user-friendly name for an enum value by checking for attributes like Display, Description, or EnumMember.
        /// </summary>
        /// <param name="enumValue">The enum value for which the display name is to be retrieved.</param>
        /// <param name="skipDisplayAttribute">Indicates whether to skip checking the Display attribute.</param>
        /// <param name="skipDescriptionAttribute">Indicates whether to skip checking the Description attribute.</param>
        /// <param name="skipEnumMemberAttribute">Indicates whether to skip checking the EnumMember attribute.</param>
        /// <returns>The user-friendly name of the enum value, or the enum name if no attributes are found.</returns>
        public static string GetDisplayName(
            this Enum enumValue,
            bool skipDisplayAttribute = false,
            bool skipDescriptionAttribute = false,
            bool skipEnumMemberAttribute = false)
        {
            MemberInfo? memberInfo = enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault();

            if (memberInfo is null)
            {
                return enumValue.ToString();
            }

            if (!skipDisplayAttribute)
            {
                // Check for DisplayAttribute
                var displayAttribute = memberInfo.GetCustomAttribute<DisplayAttribute>();
                if (!string.IsNullOrEmpty(displayAttribute?.GetName()))
                {
                    return displayAttribute.GetName()!;
                }
            }

            if (!skipDescriptionAttribute)
            {
                // Check for DescriptionAttribute
                var descriptionAttribute = memberInfo.GetCustomAttribute<DescriptionAttribute>();
                if (!string.IsNullOrEmpty(descriptionAttribute?.Description))
                {
                    return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(descriptionAttribute.Description!.ToLower());
                }
            }

            if (!skipEnumMemberAttribute)
            {
                // Check for EnumMemberAttribute
                var enumMemberAttribute = memberInfo.GetCustomAttribute<EnumMemberAttribute>();
                if (!string.IsNullOrEmpty(enumMemberAttribute?.Value))
                {
                    return enumMemberAttribute.Value!;
                }
            }

            // Fallback to the enum name
            return enumValue.ToString();
        }
    }
}
