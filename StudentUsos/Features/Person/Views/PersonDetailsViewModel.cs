using CommunityToolkit.Mvvm.ComponentModel;
using StudentUsos.Features.Person.Models;
using StudentUsos.Features.Person.Repositories;
using StudentUsos.Features.Person.Services;

namespace StudentUsos.Features.Person.Views
{
    public partial class PersonDetailsViewModel : BaseViewModel
    {
        ILecturerService lecturerService;
        ILecturerRepository lecturerRepository;
        public PersonDetailsViewModel(ILecturerService lecturerService, ILecturerRepository lecturerRepository)
        {
            this.lecturerService = lecturerService;
            this.lecturerRepository = lecturerRepository;
        }

        public void Init(Models.Person person)
        {
            Person = person;
        }

        public async Task InitAsync(Lecturer lecturer)
        {
            await Task.Delay(100);
            //lecturer passed from constructor will be most likely a simplified object containing only basic information like first name and last name
            Lecturer = lecturer;
            Person = lecturer;
            LoadDetailedLecturerAsync();
        }

        async void LoadDetailedLecturerAsync()
        {
            var lecturerLocalDb = lecturerRepository.Get(Lecturer.Id);
            if (lecturerLocalDb != null)
            {
                Lecturer = lecturerLocalDb;
                Person = lecturerLocalDb;
            }
            MainStateKey = StateKey.Loaded;

            await Task.Delay(1500);

            var lecturerApi = await lecturerService.GetDetailedLecturersAsync((string)Lecturer.Id);
            if (lecturerApi != null)
            {
                lecturerApi.InitialsBackgroundColor = lecturerLocalDb?.InitialsBackgroundColor ?? lecturerApi.InitialsBackgroundColor;
                Lecturer = lecturerApi;
                Person = lecturerApi;
                lecturerRepository.InsertOrReplace(lecturerApi);
            }
        }

        [ObservableProperty] Models.Person person;
        [ObservableProperty] Lecturer lecturer;
        [ObservableProperty] string mainStateKey = StateKey.Loading;
    }
}
