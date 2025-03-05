using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using StudentUsos.Features.Activities.Models;
using StudentUsos.Features.Person.Models;
using StudentUsos.Features.Person.Repositories;
using StudentUsos.Features.StudentProgrammes.Services;
using System.Collections.ObjectModel;

namespace StudentUsos.Features.Groups.Views;

public partial class StaffGroupDetailsViewModel : BaseViewModel
{
    IStudentProgrammeService studentProgrammeService;
    ILecturerRepository lecturerRepository;
    public StaffGroupDetailsViewModel(IStudentProgrammeService studentProgrammeService,
        ILecturerRepository lecturerRepository)
    {
        this.studentProgrammeService = studentProgrammeService;
        this.lecturerRepository = lecturerRepository;

        Activity = new Activity();
    }

    public void Init(Activity activity, Action? onClose = null)
    {
        Activity = activity;
        OnClose = onClose;

        LoadProgrammeAsync();
        LoadLecturers();
    }

    public Activity Activity { get; set; }
    [ObservableProperty] ObservableCollection<Lecturer> lecturers = new();

    [ObservableProperty] Action? onClose;

    void LoadLecturers()
    {
        string ids = Activity.LecturerIds;
        ids = RemoveUnnecessaryCharacters(ids);
        var idsSplited = ids.Split(',').ToList();

        var lecturersFromLocal = lecturerRepository.Get(idsSplited);

        Lecturers = lecturersFromLocal.ToObservableCollection();

        string RemoveUnnecessaryCharacters(string text)
        {
            string res = "";
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] >= '0' && text[i] <= '9' || text[i] == ',')
                {
                    res += text[i];
                }
            }
            return res;
        }
    }

    public string ProgrammeName { get => Utilities.GetLocalizedStringFromJson(ProgrammeNameJson); }
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgrammeName))]
    string programmeNameJson;
    public string ProgrammeFacultyId { get; set; }
    public string ProgrammeFacultyName { get => Utilities.GetLocalizedStringFromJson(ProgrammeFacultyNameJson); }
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgrammeFacultyName))]
    string programmeFacultyNameJson;

    async void LoadProgrammeAsync()
    {
        try
        {
            var splited = Activity.CourseId.Split(">");
            if (splited.Length == 0) return;
            var programme = await studentProgrammeService.GetProgrammeDetailsAsync(splited[0]);
            if (programme is null)
            {
                return;
            }
            ProgrammeNameJson = programme.NameJson;
            ProgrammeFacultyId = programme.Faculty.Id;
            ProgrammeFacultyNameJson = programme.Faculty.NameJson;
        }
        catch
        {

        }
    }
}