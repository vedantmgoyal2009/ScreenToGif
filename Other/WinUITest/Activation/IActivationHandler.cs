using System.Threading.Tasks;

namespace WinUITest.Activation;

public interface IActivationHandler
{
    bool CanHandle(object args);

    Task HandleAsync(object args);
}
