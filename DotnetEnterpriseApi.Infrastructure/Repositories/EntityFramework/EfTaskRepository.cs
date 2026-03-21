using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;
using DotnetEnterpriseApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetEnterpriseApi.Infrastructure.Repositories.EntityFramework
{
    public class EfTaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public EfTaskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TaskItem>> GetAllAsync(int? cursor, int pageSize)
        {
            var query = _context.Tasks.AsNoTracking();

            if (cursor.HasValue)
            {
                query = query.Where(t => t.Id < cursor.Value);
            }

            return await query
                .OrderByDescending(t => t.Id)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<TaskItem?> GetByIdAsync(int id)
        {
            return await _context.Tasks.FindAsync(id);
        }

        public async Task<TaskItem> AddAsync(TaskItem taskItem)
        {
            _context.Tasks.Add(taskItem);
            await _context.SaveChangesAsync();
            return taskItem;
        }

        public async Task<bool> UpdateAsync(TaskItem taskItem)
        {
            var existingTask = await _context.Tasks.FindAsync(taskItem.Id);

            if (existingTask == null)
                return false;

            existingTask.Title = taskItem.Title;
            existingTask.Description = taskItem.Description;
            existingTask.IsCompleted = taskItem.IsCompleted;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
                return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
