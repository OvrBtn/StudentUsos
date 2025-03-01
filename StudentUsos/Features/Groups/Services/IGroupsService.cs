using StudentUsos.Features.Groups.Models;

namespace StudentUsos.Features.Groups.Services
{
    public interface IGroupsService
    {
        public Task SetEctsPointsAsync(IEnumerable<Group> groups);
        /// <summary>
        /// Check whether there are any groups without defined ECTS points and set them if so
        /// </summary>
        /// <param name="groups"></param>
        /// <returns>true if there were missing ECTS points, false otherwise</returns>
        public Task<bool> SetEctsPointsIfNotSetAsync(IEnumerable<Group> groups);

        /// <summary>
        /// Get groups from USOS API for single term which is currently conducted
        /// </summary>
        /// <returns><see cref="GetCurrentTermGroupsServerResult"/> containing terms and groups returned by API</returns>
        public Task<GetCurrentTermGroupsServerResult?> GetCurrentTermGroupsServerAsync();
        /// <summary>
        /// Get groups from USOS API
        /// </summary>
        /// <param name="getOnlyActiveTerms"></param>
        /// <param name="getParticipants"></param>
        /// <returns><see cref="GetGroupedGroupsServerResult"/> containing terms, groups and groupsGrouped returned by API</returns>
        public Task<GetGroupedGroupsServerResult?> GetGroupedGroupsServerAsync(bool getOnlyActiveTerms = true, bool getParticipants = true);
    }
}
