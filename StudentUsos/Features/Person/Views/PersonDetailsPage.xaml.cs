using StudentUsos.Controls;
using StudentUsos.Features.Person.Models;

namespace StudentUsos.Features.Person.Views;

public partial class PersonDetailsPage : CustomContentPageNotAnimated
{
    PersonDetailsViewModel personDetailsViewModel;
    public PersonDetailsPage(PersonDetailsViewModel personDetailsViewModel)
    {
        InitializeComponent();
        BindingContext = this.personDetailsViewModel = personDetailsViewModel;
    }

    public void Init(Lecturer lecturer)
    {
        _ = personDetailsViewModel.InitAsync(lecturer);
    }
}