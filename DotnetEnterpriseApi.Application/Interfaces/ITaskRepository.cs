using DotnetEnterpriseApi.Domain.Entities;

namespace DotnetEnterpriseApi.Application.Interfaces
{
    public interface ITaskRepository
    {
        Task<List<TaskItem>> GetAllAsync(int pageNumber, int pageSize);

        Task<TaskItem?> GetByIdAsync(int id);

        Task<TaskItem> AddAsync(TaskItem taskItem);

        Task<bool> UpdateAsync(TaskItem taskItem);

        Task<bool> DeleteAsync(int id);
    }
}