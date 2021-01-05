using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MixSchoolManagement.Models;
using MixSchoolManagement.Models.Blogs;
using MixSchoolManagement.Models.EnumTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixSchoolManagement.Infrastructure
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            ///指定实体在数据库中生成的名称。
            modelBuilder.Entity<Course>().ToTable("Course", "School");
            modelBuilder.Entity<StudentCourse>().ToTable("StudentCourse", "School");
            modelBuilder.Entity<Person>().ToTable("Person");

            modelBuilder.Entity<Blog>().ToTable("Blogs").HasKey(a => a.Id);
            modelBuilder.Entity<Blog>().Property(a => a.Title).HasMaxLength(50).HasColumnName("BlogTitle");

            modelBuilder.Entity<CourseAssignment>()
                   .HasKey(c => new { c.CourseID, c.TeacherID });
        }
    }
}