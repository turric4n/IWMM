namespace IWMM.Settings
{
    public class Group
    {
        public Group()
        {
            Entries = new List<string>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> Entries { get; set; }
    }
}
