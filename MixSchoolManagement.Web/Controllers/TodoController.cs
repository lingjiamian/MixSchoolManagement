using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MixSchoolManagement.Infrastructure.Repositories;
using MixSchoolManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixSchoolManagement.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        //注入仓储服务
        private readonly IRepository<TodoItem, long> _todoItemRepository;

        public TodoController(IRepository<TodoItem, long> todoRepository)
        {
            this._todoItemRepository = todoRepository;
        }

        /// <summary>
        /// 获取所有待办事项
        /// </summary>
        /// <returns> </returns>
        [HttpGet]
        public async Task<ActionResult<List<TodoItem>>> GetAll()
        {      
            var models = await _todoItemRepository.GetAllListAsync();
            return models;
        }

        #region 获取指定待办事项

        /// <summary>
        /// 获取指定待办事项
        /// </summary>
        /// <param name="id"> </param>
        /// <returns> </returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetById(long id)
        {
            var todoItem = await _todoItemRepository.FirstOrDefaultAsync(a => a.Id == id);

            if (todoItem == null)
            {   
                return NotFound();
            }

            return todoItem;
        }

        #endregion 获取指定待办事项

        #region 更新指定待办事项

        /// <summary>
        /// 更新待办事项
        /// </summary>
        /// <param name="id"> </param>
        /// <param name="todoItem"> </param>
        /// <returns> </returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, TodoItem todoItem)
        {
            if (id != todoItem.Id)
            {
                return BadRequest();
            }

            await _todoItemRepository.UpdateAsync(todoItem);

            return NoContent();
        }

        #endregion 更新指定待办事项

        #region 添加待办实现

        /// <summary>
        /// 添加待办实现
        /// </summary>
        /// <param name="todoItem"> </param>
        /// <returns> </returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TodoItem>> Create(TodoItem todoItem)
        {
            await _todoItemRepository.InsertAsync(todoItem);

            //创建一个reatedAtActionResult对象，它生成一个状态码为Status201 Created的响应。
            return CreatedAtAction(nameof(GetAll), new { id = todoItem.Id }, todoItem);
        }

        #endregion 添加待办实现

        #region 删除指定的待办事项

        /// <summary>
        /// 删除指定的待办事项
        /// </summary>
        /// <param name="id"> </param>
        /// <returns> </returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<TodoItem>> Delete(long id)
        {
            var todoItem = await _todoItemRepository.FirstOrDefaultAsync(a => a.Id == id);
            if (todoItem == null)
            {
                return NotFound();
            }
            await _todoItemRepository.DeleteAsync(todoItem);
            return todoItem;
        }

        #endregion 删除指定的待办事项
    }
}