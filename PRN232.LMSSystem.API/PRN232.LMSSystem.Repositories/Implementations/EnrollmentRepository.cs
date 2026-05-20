using PRN232.LMSSystem.Repositories.Data;
using PRN232.LMSSystem.Repositories.Entities;
using PRN232.LMSSystem.Repositories.Interfaces;

namespace PRN232.LMSSystem.Repositories.Implementations;

public class EnrollmentRepository : GenericRepository<Enrollment>, IEnrollmentRepository
{
    public EnrollmentRepository(LMSDbContext context) : base(context)
    {
    }
}
