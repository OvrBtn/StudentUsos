﻿using StudentUsos.Features.Grades.Helpers;
using StudentUsos.Features.Grades.Models;

namespace StudentUsos.Features.Grades.Repositories;

public class GradesRepository : IGradesRepository
{
    ILocalDatabaseManager localDatabaseManager;
    ILogger? logger;
    public GradesRepository(ILocalDatabaseManager localDatabaseManager, ILogger? logger = null)
    {
        this.localDatabaseManager = localDatabaseManager;
        this.logger = logger;
    }

    public FinalGrade? GetLatestGrade()
    {
        try
        {
            var res = localDatabaseManager.Get<FinalGrade>(x => x.IsLatest);
            if (res == null || res.IsEmpty)
            {
                var allGrades = localDatabaseManager.GetAll<FinalGrade>();
                res = GradesHelper.FindLatest(allGrades);
            }
            if (res == null || res.IsEmpty) return null;
            return res;
        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
            return new FinalGrade();
        }
    }

    public List<FinalGrade> GetAll()
    {
        return localDatabaseManager.GetAll<FinalGrade>();
    }

    public FinalGrade? Get(string courseUnitId)
    {
        return localDatabaseManager.Get<FinalGrade>(x => x.CourseUnitId == courseUnitId);
    }

    public void InsertOrReplace(FinalGrade finalGrade)
    {
        localDatabaseManager.InsertOrReplace(finalGrade);
    }

    public void DeleteAll()
    {
        localDatabaseManager.ClearTable<FinalGrade>();
    }

    public void InsertAll(IEnumerable<FinalGrade> finalGrades)
    {
        localDatabaseManager.InsertAll(finalGrades);
    }
}