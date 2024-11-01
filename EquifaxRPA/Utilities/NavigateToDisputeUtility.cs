﻿using Equifax.Api.Helper;
using OpenQA.Selenium;
using System;

namespace Equifax.Api.Utilities
{
    public class NavigateToDisputeUtility
    {
        private readonly ElementLoader _elementLoader;
        private readonly SleepLoader _sleepLoader;

        public NavigateToDisputeUtility()
        {
            _elementLoader = new ElementLoader();
            _sleepLoader = new SleepLoader();
        }


        public void NavigateToDispute(IWebDriver driver)
        {
            try
            {
                string disputeCenterXPath = "//*[@id='fullDisputeLink']";

                _elementLoader.Load(disputeCenterXPath, driver);

                Console.WriteLine("Navigated to dispute tab.");
                _sleepLoader.Seconds(3);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation Error: {ex.Message}");
                throw ex;
            }
        }
    }
}
