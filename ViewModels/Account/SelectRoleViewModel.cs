namespace Golestan.ViewModels.Account
{
    public class SelectRoleViewModel
    {
        public int UserId { get; set; }

        // لیستی از نقش‌ها و شناسه‌های پروفایل مربوطه
        public List<RoleProfileItem> Roles { get; set; } = new List<RoleProfileItem>();

        // نقش انتخاب شده برای ارسال به سرور (شناسه پروفایل)
        public int SelectedProfileId { get; set; }
    }

    public class RoleProfileItem
    {
        public int ProfileId { get; set; } // شناسه پروفایل (مثلا StudentId یا InstructorId)
        public string RoleName { get; set; } // مثل "دانشجو" یا "استاد"
        public string DisplayName { get; set; } // مثلاً نام دانشجو یا استاد برای نمایش
    }


}
