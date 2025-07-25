using Microsoft.AspNetCore.Mvc.Rendering;

namespace Golestan.ViewModels
{
    public class SectionDetailViewModel
    {
        public int SectionId { get; set; }

        public string CourseTitle { get; set; }
        public string ClassroomInfo { get; set; }
        public string TimeSlotInfo { get; set; }

        public DateTime FinalExamDate { get; set; }

        public int? InstructorId { get; set; }
        public string InstructorName { get; set; }

        public List<SelectListItem> AvailableInstructors { get; set; }
        public List<SelectListItem> AvailableStudents { get; set; }

        public List<StudentViewModel> EnrolledStudents { get; set; }
    }

}
