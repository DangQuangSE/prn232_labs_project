using PRN232.LMSSystem.Repositories.Data;
using PRN232.LMSSystem.Repositories.Entities;
using PRN232.LMSSystem.Repositories.Interfaces;

namespace PRN232.LMSSystem.Repositories.Implementations;

public class StudentRepository : GenericRepository<Student>, IStudentRepository
{
    public StudentRepository(LMSDbContext context) : base(context)
    {
    }
}
