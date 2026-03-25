<<<<<<< HEAD
﻿using MvcProject.Models.Domain;
=======
using MvcProject.Models.Domain;
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2

namespace MvcProject.Repositories.Interfaces
{
    public interface ICommentRepository: IRepository<TaskComment>
    {
<<<<<<< HEAD
=======
        Task<IEnumerable<TaskComment>> GetCommentsByTaskIdAsync(int taskId);
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
    }
}
