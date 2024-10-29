using Equifax.Api.Domain.DTOs;
using Equifax.Api.Helper;
using OpenQA.Selenium;
using System;

namespace Equifax.Api.Utilities
{
    public class LoginUtility
    {
        private readonly SleepLoader _sleepLoader;

        public LoginUtility()
        {
            _sleepLoader = new SleepLoader();
        }


        public void Login(LoginCredentialRequestDto request, IWebDriver driver)
        {
            try
            {
                // Now you can find the elements
                var emailInput = driver.FindElement(By.Id("login-email"));
                var passwordInput = driver.FindElement(By.Id("login-password"));
                var submitButton = driver.FindElement(By.Id("login-button"));

                // Enter email and password
                emailInput.SendKeys(request.user_name);
                passwordInput.SendKeys(request.user_password);
                submitButton.Click();


                Console.WriteLine("Login successful.");
                _sleepLoader.Seconds(3);

                // Check if the subscription page is shown (span with price $19.95)
                TryHandleSubscriptionPrompt(driver);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login Error: {ex.Message}");
                throw ex;
            }
        }


        //---------------------------HELPER FUNCTIONS------------------------------//
        private void TryHandleSubscriptionPrompt(IWebDriver driver)
        {
            try
            {
                // Try to find the decline button on the subscription prompt
                IWebElement declineButton = driver.FindElement(By.XPath("//*[@id='no']"));
                declineButton.Click();
                Console.WriteLine("Declined subscription offer.");
            }
            catch (NoSuchElementException)
            {
                // The subscription button was not found
                Console.WriteLine("No subscription page detected, continuing...");
            }
            catch (Exception ex)
            {
                // Handle any other unexpected exceptions here
                Console.WriteLine($"Error handling subscription prompt: {ex.Message}");
            }
        }
    }
}
