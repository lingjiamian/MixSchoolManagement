using MixSchoolManagement.Application.Courses.Dtos;
using MixSchoolManagement.Application.Dtos;
using MixSchoolManagement.Application.Students;
using MixSchoolManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixSchoolManagement.Application.Courses
{
    public interface ICourseService
    {
        Task<PagedResultDto<Course>> GetPaginatedResult(GetCourseInput input);

    }
}
