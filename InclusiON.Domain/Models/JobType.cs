using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models;

public class JobType : NameableEntity
{
    public virtual ICollection<BackgroundJob> BackgroundJobs { get; set; }

    public JobType()
    {
        BackgroundJobs = new HashSet<BackgroundJob>();
    }
}
