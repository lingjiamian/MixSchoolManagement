using MixSchoolManagement.Application.Dtos;
using MixSchoolManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixSchoolManagement.Application.Teachers
{
    public interface ITeacherService
    {
        /// <summary>
        /// 获取老师的分页信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<PagedResultDto<Teacher>> GetPagedTeacherList(GetTeacherInput input);
        
    }
}
