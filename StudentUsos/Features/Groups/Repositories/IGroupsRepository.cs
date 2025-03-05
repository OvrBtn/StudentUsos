using StudentUsos.Features.AcademicTerms.Models;
using StudentUsos.Features.Groups.Models;

namespace StudentUsos.Features.Groups.Repositories;

public interface IGroupsRepository
{
    /// <summary>
    /// Get groups from local database for given term for courses which are currently conducted
    /// </summary>
    /// <param name="term"></param>
    /// <returns>List of groups</returns>
    public List<Group> GetGroups(Term term);
    public List<Group> GetAll();

    public Group? GetGroup(string courseId, string classTypeName);

    /// <summary>
    /// Get grouped groups for all active terms
    /// </summary>
    /// <returns></returns>
    public List<GroupsGrouped> GetActiveTermsGroupsGrouped();

    public void InsertOrReplace(IEnumerable<Term> terms);
    public void InsertOrReplace(IEnumerable<Group> groups);
    public void InsertOrReplace(Group groups);
}