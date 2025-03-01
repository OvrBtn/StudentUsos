namespace StudentUsos.Features.Grades.Models
{
    public class ChartData
    {
        public string Label { get; set; }
        public int Value { get; set; }
        public float ValueScaled { get; set; } = 0;

        public ChartData(string label, int value)
        {
            Label = label;
            Value = value;
        }
    }
}
