using Equifax.Api.Helper;
using OpenQA.Selenium;
using System;

namespace Equifax.Api.Utilities
{
    public class OpenBrowserAndNavigateUtility
    {
        private readonly SleepLoader _sleepLoader;

        public OpenBrowserAndNavigateUtility()
        {
            _sleepLoader = new SleepLoader();
        }

        public void OpenBrowserAndNavigate(string url, IWebDriver driver)
        {
            try
            {
                driver.Navigate().GoToUrl(url);

                Console.WriteLine($"Navigated to: {url}");
                _sleepLoader.Seconds(3);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw ex;
            }
        }
    }
}
