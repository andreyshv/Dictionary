namespace Models
{
    public class Settings
    {
        public int Id { get; set; }
        public int SizeSet { get; set; }

        public void SetDefaults()
        {
            SizeSet = 20;
        }
    }
}
