namespace DemoMVC.Models
{
    public class Student
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string School { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;
    }
}