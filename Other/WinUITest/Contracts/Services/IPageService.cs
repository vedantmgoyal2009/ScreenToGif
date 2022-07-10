using System;

namespace WinUITest.Contracts.Services;

public interface IPageService
{
    Type GetPageType(string key);
}
