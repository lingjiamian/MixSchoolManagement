using MixSchoolManagement.Application.Dtos;

namespace MixSchoolManagement.Application.Teachers
{
    public class GetDepartmentInput : PagedSortedAndFilterInput
    {
        public GetDepartmentInput()
        {
            Sorting = "Name";
            MaxResultCount = 3;
        }
    }
}