using PRN232.LMSSystem.Repositories.Data;
using PRN232.LMSSystem.Repositories.Entities;
using PRN232.LMSSystem.Repositories.Interfaces;

namespace PRN232.LMSSystem.Repositories.Implementations;

public class SubjectRepository : GenericRepository<Subject>, ISubjectRepository
{
    public SubjectRepository(LMSDbContext context) : base(context)
    {
    }
}
