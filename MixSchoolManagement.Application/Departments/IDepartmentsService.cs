using MixSchoolManagement.Application.Dtos;
using MixSchoolManagement.Models;
using System.Threading.Tasks;

namespace MixSchoolManagement.Application.Teachers
{
    public interface IDepartmentsService
    {
        /// <summary>
        /// 获取院系的分页信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<PagedResultDto<Department>> GetPagedDepartmentsList(GetDepartmentInput input);
    }
}