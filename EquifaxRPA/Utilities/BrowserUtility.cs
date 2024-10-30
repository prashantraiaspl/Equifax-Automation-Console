using OpenQA.Selenium;
using Equifax.Api.Domain.DTOs;
using Equifax.Api.Helper;
using System.Threading.Tasks;
using System;

namespace Equifax.Api.Utilities
{
    public class BrowserUtility
    {
        private readonly DriverSetupManager _driverSetupManager;
        private readonly OpenBrowserAndNavigateUtility _openBrowserAndNavigateUtility;
        private readonly LoginUtility _loginUtility;
        private readonly NavigateToDisputeUtility _navigateToDisputeUtility;
        private readonly FileDisputeUtility _fileDisputeUtility;
        private readonly CloseBrowserUtility _closeBrowserUtility;
        private readonly SleepLoader _sleepLoader;

        public BrowserUtility()
        {
            _driverSetupManager = new DriverSetupManager();
            _openBrowserAndNavigateUtility = new OpenBrowserAndNavigateUtility();
            _loginUtility = new LoginUtility();
            _navigateToDisputeUtility = new NavigateToDisputeUtility();
            _fileDisputeUtility = new FileDisputeUtility();
            _closeBrowserUtility = new CloseBrowserUtility();
            _sleepLoader = new SleepLoader();
        }



        public async Task<ResponseBody> BrowserAutomationProcess(string url, LoginCredentialRequestDto loginCredentials, DisputeRequestDto disputeRequest)
        {
            ResponseBody response = new ResponseBody();

            try
            {
                IWebDriver driver = null;

                //-------------INITIALIZATION OF CHROME DRIVER-------------//
                driver = _driverSetupManager.InitializeDriver();

                _sleepLoader.Seconds(3);

                //-------------Step 1: Open URL-------------//
                _openBrowserAndNavigateUtility.OpenBrowserAndNavigate(url, driver);

                //-------------Step 2: Perform Login-------------//
                _loginUtility.Login(loginCredentials, driver);

                _sleepLoader.Seconds(5);

                //-------------Step 3: Navigate to Dispute-------------//
                _navigateToDisputeUtility.NavigateToDispute(driver);

                _sleepLoader.Seconds(5);

                //-------------Step 4: File a Dispute-------------//
                response = await _fileDisputeUtility.FileDisputeAsync(disputeRequest, driver);

                _sleepLoader.Seconds(5);

                //-------------Step 5: Close the Browser-------------//
                _closeBrowserUtility.CloseBrowser(driver);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during browser interaction: {ex.Message}");

                response.status = false;
                response.message = ex.ToString();
            }

            return response;
        }
    }

}
