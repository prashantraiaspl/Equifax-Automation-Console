using OpenQA.Selenium;
using System;

namespace Equifax.Api.Helper
{
    public class ElementLoader
    {
        private readonly SleepLoader _sleepLoader;

        public ElementLoader()
        {
            _sleepLoader = new SleepLoader();
        }


        public void Load(string xPath, IWebDriver driver)
        {
			try
			{
                var elements = driver.FindElements(By.XPath(xPath));

                if (elements.Count > 0)
                {
                    elements[0].Click();
                    Console.WriteLine("------ELement is Clicked (if).------");
                }
                else
                {
                    _sleepLoader.Seconds(10);
                    var element = driver.FindElement(By.XPath(xPath));
                    _sleepLoader.Seconds(10);

                    element.Click();
                    Console.WriteLine("------ELement is Clicked (else).------");
                }
            }
			catch (Exception ex)
			{
                Console.WriteLine($"Element Loading Error: {ex.Message}");
            }
        }
    }
}
