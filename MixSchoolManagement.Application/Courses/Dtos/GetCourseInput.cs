﻿using MixSchoolManagement.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixSchoolManagement.Application.Courses.Dtos
{
    public class GetCourseInput : PagedSortedAndFilterInput
    {
        public GetCourseInput()
        {          
                Sorting = "CourseID";
            MaxResultCount = 5;
        }
    }
}
