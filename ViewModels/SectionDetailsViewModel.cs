namespace Golestan.ViewModels
{
    public class SectionDetailsViewModel
    {
        public int Id { get; set; }

        public string CourseTitle { get; set; }
        public int Year { get; set; }
        public int Semester { get; set; }

        public string ClassroomName { get; set; }
        public string TimeSlotInfo { get; set; }

        public DateTime FinalExamDate { get; set; }

        public string? InstructorName { get; set; } // null یعنی استاد نداره

        public List<StudentInSectionViewModel> Students { get; set; }
    }

    public class StudentInSectionViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
    }

}
