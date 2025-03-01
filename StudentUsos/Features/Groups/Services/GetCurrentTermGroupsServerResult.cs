using StudentUsos.Features.AcademicTerms.Models;
using StudentUsos.Features.Groups.Models;

namespace StudentUsos.Features.Groups.Services
{
    public class GetCurrentTermGroupsServerResult
    {
        public List<Term> Terms = new();
        public List<Group> Groups = new();
    }
}
