using SQLite;
using StudentUsos.Features.StudentProgrammes.Services.JsonModels;

namespace StudentUsos.Features.StudentProgrammes.Models;

/// <summary>
/// equivalent to USOS' services/progs module
/// </summary>
public class StudentProgramme
{
    //from services/progs/student
    [PrimaryKey]
    //e.g. "S1Inf1"
    public string ProgrammeId { get; set; }
    public string DescriptionJson { get; set; }
    public string Status { get; set; }
    public string AdmissionDate { get; set; }
    public string IsPrimary { get; set; }

    //additional data from services/progs/programme
    public string FacultyId { get; set; }
    public string FacultyNameJson { get; set; }

    //internal, used for checking how old the data is
    public string CreationDate { get; set; }

    public StudentProgramme()
    {

    }

    public void AssignProgrammeDetailsJson(ProgrammeDetailsJson programme)
    {
        try
        {
            FacultyId = programme.Faculty.Id;
            FacultyNameJson = programme.Faculty.NameJson;
            CreationDate = DateAndTimeProvider.Current.Now.ToString();
        }
        catch (Exception ex) { Logger.Default?.LogCatchedException(ex); }
    }
}