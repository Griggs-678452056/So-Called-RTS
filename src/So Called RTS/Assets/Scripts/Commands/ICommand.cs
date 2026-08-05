namespace Scripts.Commands
{
    public interface ICommand
    {
        bool CanHandle(CommandContext context);
        void Handle(CommandContext context);
    }
}
