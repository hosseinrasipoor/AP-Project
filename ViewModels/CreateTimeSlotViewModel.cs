using System;
using System.ComponentModel.DataAnnotations;

namespace Golestan.ViewModels
{
    

    public class CreateTimeSlotViewModel
    {
        [Required(ErrorMessage = "لطفاً روز را وارد کنید.")]
        public string Day { get; set; }

        [Required(ErrorMessage = "لطفاً زمان شروع را وارد کنید.")]
        [DataType(DataType.Time)]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "لطفاً زمان پایان را وارد کنید.")]
        [DataType(DataType.Time)]
        [CustomValidation(typeof(CreateTimeSlotViewModel), nameof(ValidateTimeRange))]
        public DateTime EndTime { get; set; }

        public static ValidationResult ValidateTimeRange(DateTime endTime, ValidationContext context)
        {
            var instance = context.ObjectInstance as CreateTimeSlotViewModel;
            if (instance == null || instance.StartTime >= endTime)
            {
                return new ValidationResult("زمان پایان باید بعد از زمان شروع باشد.");
            }

            return ValidationResult.Success;
        }
    }

}
