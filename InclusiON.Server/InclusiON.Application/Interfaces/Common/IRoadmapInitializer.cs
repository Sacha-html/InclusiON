using System;
using System.Threading;
using System.Threading.Tasks;

namespace InclusiON.Application.Interfaces.Common
{
    public interface IRoadmapInitializer
    {
        Task InitializeStudentRoadmapAsync(Guid studentId, Guid? supervisorUserId = null, CancellationToken cancellationToken = default);
    }
}
