namespace StudentUsos.Features.Groups.Models
{
    public class GroupsGrouped : List<Group>, IEquatable<GroupsGrouped>
    {
        public string TermId { get; set; }
        public string TermName { get; set; }

        public GroupsGrouped(string termId, string termName, List<Group> groups) : base(groups)
        {
            TermId = termId;
            TermName = termName;
        }

        public static bool AreEqual(GroupsGrouped g1, GroupsGrouped g2)
        {
            return g1.Equals(g2);
        }

        public bool Equals(GroupsGrouped? other)
        {
            if (other is null)
            {
                return false;
            }
            if (this.TermId != other.TermId || this.Count != other.Count) return false;
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].Equals(other[i]) == false)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
