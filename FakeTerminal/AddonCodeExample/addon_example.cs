namespace FakeTerminalExampleAddon
{
    public class ExampleAddon : ICustomCommand
    {
        public void Register()
        {
            CommandSystem commandSystem = new CommandSystem();
            commandSystem.AddCommand("test", "Test commnad", "custom", Execute, false);
        }

        private void Execute(string[] args)
        {
            Console.WriteLine("Example Loaded");
        }
    }
}