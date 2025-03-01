using StudentUsos.Controls;
using StudentUsos.Features.Grades.Models;

namespace StudentUsos.Features.Grades.Views
{
    public partial class ModifyGradePage : CustomBottomSheet
    {
        public FinalGradeGroup FinalGradeGroup { get; private set; }
        ModifyGradeViewModel modifyGradeViewModel;
        public ModifyGradePage(ModifyGradeViewModel modifyGradeViewModel)
        {
            InitializeComponent();
            BindingContext = this.modifyGradeViewModel = modifyGradeViewModel;
        }

        public void Init(FinalGradeGroup finalGradeGroup, Action valueChanged)
        {
            FinalGradeGroup = finalGradeGroup;
            modifyGradeViewModel.Init(this, finalGradeGroup, valueChanged);
        }
    }
}