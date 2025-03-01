using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using StudentUsos.Features.Grades.Models;

namespace StudentUsos.Features.Grades.Views
{
    public partial class ModifyGradeViewModel : BaseViewModel
    {
        [ObservableProperty] FinalGradeGroup finalGradeGroup;
        Action valueChanged;
        ModifyGradePage modifyGradePage;
        public ModifyGradeViewModel()
        {
            EditFirstTermCommand = new((arg) => Edit(arg, Editing.FirstTerm));
            EditSecondTermCommand = new((arg) => Edit(arg, Editing.SecondTerm));
        }

        public void Init(ModifyGradePage modifyGradePage, FinalGradeGroup finalGradeGroup, Action valueChanged)
        {
            FinalGradeGroup = finalGradeGroup;

            this.valueChanged = valueChanged;
            this.modifyGradePage = modifyGradePage;
        }

        [ObservableProperty] Command<string> editFirstTermCommand;
        [ObservableProperty] Command<string> editSecondTermCommand;

        enum Editing
        {
            FirstTerm,
            SecondTerm
        }

        //public ObservableCollection<string> Options { get; set; } = new() { "NB", "NK", "2.0", "2.5", "3.0", "3.5", "4.0", "4.5", "5.0" };
        [ObservableProperty]
        ObservableCollection<Option> options = new()
        {
            new("NB"),
            new("NK"),
            new("2.0"),
            new("2.5"),
            new("3.0"),
            new("3.5"),
            new("4.0"),
            new("4.5"),
            new("5.0")
        };
        void Edit(string newValue, Editing editing)
        {
            if (editing == Editing.FirstTerm)
            {
                FinalGradeGroup.FirstTermGrade.ValueSymbol = newValue;
                FinalGradeGroup.FirstTermGrade.IsModified = true;
            }
            else
            {
                FinalGradeGroup.SecondTermGrade.ValueSymbol = newValue;
                FinalGradeGroup.SecondTermGrade.IsModified = true;
            }
            valueChanged?.Invoke();
        }
    }

    public class Option
    {
        public string Value { get; set; }
        public Option(string value)
        {
            Value = value;
        }
    }

}
