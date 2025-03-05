using StudentUsos.Features.AcademicTerms.Models;
using StudentUsos.Features.Groups.Models;

namespace StudentUsos.Features.Groups.Services;

public class GetGroupedGroupsServerResult
{
    public List<Term> Terms = new();
    public List<Group> Groups = new();
    public List<GroupsGrouped> GroupsGrouped = new();
}