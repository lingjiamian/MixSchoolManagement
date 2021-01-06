using MixSchoolManagement.Application.Dtos;
using System.ComponentModel.DataAnnotations;

namespace MixSchoolManagement.Application.Students {


    public class GetStudentInput : PagedSortedAndFilterInput
    {            

        public GetStudentInput()
        {
            Sorting = "Id";
        }
    }
}