using System.Threading.Tasks;

namespace WinUITest.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
