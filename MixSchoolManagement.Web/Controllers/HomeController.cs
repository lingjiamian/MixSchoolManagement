using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MixSchoolManagement.Application.Students;
using MixSchoolManagement.Application.Students.Dtos;
using MixSchoolManagement.EntityFrameworkCore;
using MixSchoolManagement.Infrastructure.Repositories;
using MixSchoolManagement.Models;
using MixSchoolManagement.Security.CustomTokenProvider;
using MixSchoolManagement.ViewModels;
using MixSchoolManagement.Web.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MixSchoolManagement.Web.Controllers
{
    [Authorize(Policy = "EditRolePolicy")]
    [Consumes("application/xml")]
    public class HomeController : Controller
    {
        private readonly IRepository<Student, int> _studentRepository;
        private readonly AppDbContext _dbcontext;

        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger logger;
        private readonly IStudentService _studentService;

        //IDataProtector可以对数据进行加密或者解密，这里用来加密学生Id
        private readonly IDataProtector _protector;

        public HomeController(IWebHostEnvironment webHostEnvironment, ILogger<HomeController> logger, IDataProtectionProvider dataProtectionProvider, DataProtectionPurposeStrings dataProtectionPurposeStrings, IRepository<Student, int> studentRepository, IStudentService studentService, AppDbContext dbcontext)
        {
            _webHostEnvironment = webHostEnvironment;
            this.logger = logger;
            _studentRepository = studentRepository;
            _protector = dataProtectionProvider.CreateProtector(
                 dataProtectionPurposeStrings.StudentIdRouteValue);
            _studentService = studentService;
            _dbcontext = dbcontext;
        }

        public async Task<IActionResult> Index(GetStudentInput input)
        {
            logger.Log(LogLevel.Warning, "消息了");
            //获取分页结果
            var dtos = await _studentService.GetPaginatedResult(input);
            dtos.Data = dtos.Data.Select(s =>
            {
                //加密ID值并存储在EncryptedId属性中
                s.EncryptedId = _protector.Protect(s.Id.ToString());
                return s;
            }).ToList();
            return View(dtos);
        }

        // Details视图接收加密后的StudentID
        public ViewResult Details(string id)
        {
            var student = DecryptedStudent(id);

            if (student == null)
            {
                ViewBag.ErrorMessage = $"学生Id={id}的信息不存在，请重试。";
                return View("NotFound");
            }
            //实例化HomeDetailsViewModel并存储Student详细信息和PageTitle
            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {
                Student = student,
                PageTitle = "学生详情"
            };
            homeDetailsViewModel.Student.EncryptedId =
                _protector.Protect(student.Id.ToString());

            //将ViewModel对象传递给View()方法
            return View(homeDetailsViewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(StudentCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                //上传图片
                var uniqueFileName = ProcessUploadedFile(model);
                Student newStudent = new Student
                {
                    Name = model.Name,
                    Email = model.Email,
                    Major = model.Major,
                    EnrollmentDate = model.EnrollmentDate,
                    //将文件名保存在student对象的PhotoPath属性中
                    PhotoPath = uniqueFileName
                };

                _studentRepository.Insert(newStudent);

                var encryptedId = _protector.Protect(newStudent.Id.ToString());

                return RedirectToAction("Details", new { id = encryptedId });
            }
            return View();
        }

        [HttpGet]
        public ViewResult Edit(string id)
        {
            var student = DecryptedStudent(id);
            if (student == null)
            {
                ViewBag.ErrorMessage = $"学生Id={id}的信息不存在，请重试。";
                return View("NotFound");
            }
            StudentEditViewModel studentEditViewModel = new StudentEditViewModel
            {
                Id = id,
                Name = student.Name,
                Email = student.Email,
                Major = student.Major,
                ExistingPhotoPath = student.PhotoPath,
                EnrollmentDate = student.EnrollmentDate,
            };
            return View(studentEditViewModel);
        }

        //StudentEditViewModel 会接收来自Post请求的Edit表单数据
        [HttpPost]
        public IActionResult Edit(StudentEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var student = DecryptedStudent(model.Id);

                //用模型对象中的数据更新student对象
                student.Name = model.Name;
                student.Email = model.Email;
                student.Major = model.Major;
                student.EnrollmentDate = model.EnrollmentDate;
                
                if (model.Photos != null && model.Photos.Count > 0)
                {
                    //检查当前学生信息中是否有照片，有的话就会删除它
                    if (model.ExistingPhotoPath != null)
                    {
                        string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "avatars", model.ExistingPhotoPath);

                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }

                    //将保存新的照片到wwwroot/images/avatars文件夹中
                    student.PhotoPath = ProcessUploadedFile(model);
                }

                Student updatedstudent = _studentRepository.Update(student);

                return RedirectToAction("index");
            }

            return View(model);
        }

        #region 删除功能

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var student = await _studentRepository.FirstOrDefaultAsync(a => a.Id == id);

            if (student == null)
            {
                ViewBag.ErrorMessage = $"无法找到ID为{id}的学生信息";
                return View("NotFound");
            }

            await _studentRepository.DeleteAsync(a => a.Id == id);
            return RedirectToAction("Index");
        }

        #endregion 删除功能

       

        public async Task<ActionResult> About()
        {
            List<EnrollmentDateGroupDto> groups = new List<EnrollmentDateGroupDto>();
            //获取数据库的上下文链接
            var conn = _dbcontext.Database.GetDbConnection();
            try
            {    //打开数据库链接
                await conn.OpenAsync();
                //建立链接，因为非委托资源所以需要使用using进行内存资源的释放
                using (var command = conn.CreateCommand())
                {
                    string query = "SELECT EnrollmentDate, COUNT(*) AS StudentCount   FROM Person  WHERE Discriminator = 'Student'  GROUP BY EnrollmentDate";
                    command.CommandText = query; //赋值需要执行的sql命令
                    DbDataReader reader = await command.ExecuteReaderAsync();//执行命令
                    if (reader.HasRows)
                    {      
                        //读取数据并填充到DTO中
                        while (await reader.ReadAsync())
                        {
                            var row = new EnrollmentDateGroupDto
                            {
                                EnrollmentDate = reader.GetDateTime(0),
                                StudentCount = reader.GetInt32(1)
                            };
                            groups.Add(row);
                        }
                    }
                    //释放使用的所有的资源
                    await reader.DisposeAsync();
                }
            }
            finally
            {  
                await conn.CloseAsync();
            }
            return View(groups);
        }

        #region 私有方法

        /// <summary>
        /// 解密学生信息
        /// </summary>
        /// <param name="Id"> </param>
        /// <returns> </returns>
        private Student DecryptedStudent(string Id)
        {
            //使用 Unprotect()方法来解析学生Id
            string decryptedId = _protector.Unprotect(Id);
            int decryptedStudentId = Convert.ToInt32(decryptedId);
            Student student = _studentRepository.FirstOrDefault(s => s.Id == decryptedStudentId);
            return student;
        }

        /// <summary>
        /// 将照片保存到指定的路径中并返回文件名
        /// </summary>
        /// <returns> </returns>
        private string ProcessUploadedFile(StudentCreateViewModel model)
        {
            string uniqueFileName = null;

            if (model.Photos != null && model.Photos.Count > 0)
            {
                foreach (var photo in model.Photos)
                {
                    //TODO:文件检测
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "avatars");

                    //加GUID确保文件名唯一
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;

                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        photo.CopyTo(fileStream);
                    }
                }
            }
            return uniqueFileName;
        }

        #endregion 私有方法
    }
}
