using MvcProject.Services.Interfaces;

namespace MvcProject.Services
{
    public class DashboardService : IDashboardService
    {
        private IUnitOfWork _unitOfWork;
        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
       



    }
}
