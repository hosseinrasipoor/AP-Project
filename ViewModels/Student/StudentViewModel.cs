namespace Golestan.ViewModels
{
    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; }
        public List<StudentCourseViewModel> Courses { get; set; }
        public double GPA { get; set; }
    }

    public class StudentCourseViewModel
    {
        public string CourseTitle { get; set; }
        public double Grade { get; set; }
    }
}
