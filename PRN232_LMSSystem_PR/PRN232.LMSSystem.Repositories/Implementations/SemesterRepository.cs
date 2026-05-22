using PRN232.LMSSystem.Repositories.Data;
using PRN232.LMSSystem.Repositories.Entities;
using PRN232.LMSSystem.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.LMSSystem.Repositories.Implementations
{
    public class SemesterRepository : GenericRepository<Semester>, ISemesterRepository
    {
        public SemesterRepository(LmsLab1Context context) : base(context)
        {
        }
    }
}
