using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models;

public class BackgroundJobStatus : NameableEntity
{
    public virtual ICollection<BackgroundJob> BackgroundJobs { get; set; }

    public BackgroundJobStatus()
    {
        BackgroundJobs = new HashSet<BackgroundJob>();
    }
}
