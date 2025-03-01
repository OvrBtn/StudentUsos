using StudentUsos.Features.AcademicTerms.Models;
using StudentUsos.Features.Groups.Models;

namespace StudentUsos.Features.Groups.Repositories
{
    public class GroupsRepository : IGroupsRepository
    {
        ILocalDatabaseManager localDatabaseManager;
        public GroupsRepository(ILocalDatabaseManager localDatabaseManager)
        {
            this.localDatabaseManager = localDatabaseManager;
        }

        public List<Group> GetGroups(Term term)
        {
            return localDatabaseManager.GetAll<Group>(x => x.CourseIsCurrentlyConducted == "1" && x.TermId == term.Id);
        }

        public Group? GetGroup(string courseId, string classTypeName)
        {
            return localDatabaseManager.Get<Group>(x => x.CourseId == courseId && x.ClassType == classTypeName); ;
        }

        public List<Group> GetAll()
        {
            return localDatabaseManager.GetAll<Group>();
        }

        public List<GroupsGrouped> GetActiveTermsGroupsGrouped()
        {
            var groups = localDatabaseManager.GetAll<Group>(x => x.CourseIsCurrentlyConducted == "1");
            return GroupGroups(groups);
        }

        /// <summary>
        /// Group groups with the same term id
        /// </summary>
        /// <param name="groups"></param>
        /// <returns>Grouped groups or empty list if failed</returns>
        List<GroupsGrouped> GroupGroups(List<Group> groups)
        {
            try
            {
                List<GroupsGrouped> groupsGrouped = new();
                List<string> idsOfAddedTerms = new();
                List<Term> terms = localDatabaseManager.GetAll<Term>();
                for (int i = 0; i < groups.Count; i++)
                {
                    string currentTermId = groups[i].TermId;
                    if (idsOfAddedTerms.Contains(currentTermId) == false)
                    {
                        var found = terms.Where(x => x.Id == currentTermId).First();
                        string currentTermName = found.Name;
                        GroupsGrouped groupsGroupedObject = new(currentTermId, currentTermName, groups.Where(x => x.TermId == currentTermId).ToList());
                        groupsGrouped.Add(groupsGroupedObject);
                        idsOfAddedTerms.Add(currentTermId);
                    }
                }
                return groupsGrouped;
            }
            catch (Exception ex) { Utilities.ShowError(ex); return new List<GroupsGrouped>(); }
        }

        public void InsertOrReplace(IEnumerable<Term> terms)
        {
            localDatabaseManager.InsertOrReplaceAll(terms);
        }

        public void InsertOrReplace(IEnumerable<Group> groups)
        {
            localDatabaseManager.InsertOrReplaceAll(groups);
        }

        public void InsertOrReplace(Group group)
        {
            localDatabaseManager.InsertOrReplace(group);
        }
    }
}
