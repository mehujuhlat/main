using client.Models;
using System.ComponentModel.DataAnnotations;

namespace client.App_Code
{
    public class UniqueNicknameAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var dbContext = (MehujuhlatContext)validationContext.GetService(typeof(MehujuhlatContext));
            var nickname = value?.ToString();

            if (!string.IsNullOrEmpty(nickname))
            {
                var isInUse = dbContext.Users.Any(u => u.Nickname == nickname);
                if (isInUse)
                {
                    return new ValidationResult("Lempinimi on jo käytössä");
                }
            }

            return ValidationResult.Success;
        }
    }
}
