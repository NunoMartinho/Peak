namespace Tests.Unit
{
    public class FileTestsFixture : IDisposable
    {
        public string TestFilePath { get; private set; }

        public FileTestsFixture()
        {
            TestFilePath = Path.Combine("track.log");
            File.Create(TestFilePath).Close();
        }

        public void Dispose()
        {
            if (File.Exists(TestFilePath))
            {
                File.Delete(TestFilePath);
            }
        }
    }
}
