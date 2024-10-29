using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System.Collections.Generic;
using System;
using System.Configuration;
using OpenQA.Selenium.Chrome.ChromeDriverExtensions;
using System.Threading;

namespace Equifax.Api.Utilities
{
    public class DriverSetupManager
    {
        public IWebDriver InitializeDriver()
        {
            try
            {
                //Path for CHROME DRIVER
                string driverPath = ConfigurationManager.AppSettings["driverPath"].ToString();

                // Proxy settings
                string strProxyUrl = ConfigurationManager.AppSettings["strProxyUrl"];
                string strPort = ConfigurationManager.AppSettings["strPort"];
                string strUsername = ConfigurationManager.AppSettings["strUsername"];
                string strPassword = ConfigurationManager.AppSettings["strPassword"];


                // Initialize Chrome Options
                var chromeOptions = new ChromeOptions();

                chromeOptions.AddArgument("--start-maximized");

                chromeOptions.AddHttpProxy(strProxyUrl, Convert.ToInt32(strPort), strUsername, strPassword);

                var driver = new ChromeDriver(driverPath, chromeOptions);

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5000)); 

                return driver;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw ex;
            }
        }
    }
}
