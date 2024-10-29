using Equifax.Api.Domain.DTOs;
using Equifax.Api.Helper;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Equifax.Api.Utilities
{
    public class FileDisputeUtility
    {
        private readonly ElementLoader _elementLoader;
        private readonly BlocksLoader _blockLoader;
        private readonly BlockMatchingLoader _blockMatchingLoader;
        private readonly CheckboxLoader _checkboxLoader;
        private readonly SleepLoader _sleepLoader;

        public FileDisputeUtility ()
        {
            _elementLoader = new ElementLoader();
            _blockLoader = new BlocksLoader();
            _blockMatchingLoader = new BlockMatchingLoader();
            _checkboxLoader = new CheckboxLoader();
            _sleepLoader = new SleepLoader();
        }


        public async Task<ResponseBody> FileDisputeAsync(DisputeRequestDto disputeRequest, IWebDriver driver)
        {
            List<(IWebElement Element, int Index, string Type)> blockElementsWithIndex = new List<(IWebElement Element, int Index, string Type)>();

            ResponseBody response = new ResponseBody();

            // CONFIRMATION NUMBER
            string file_number = "";
            DateTime? submitted_date = null;

            try
            {
                // Declaring Elements XPath
                string fileDisputeXPath = "//*[@id=\"file-distupe-section-file-a-dispute-button\"]";
                string checkboxXPath = "//*[@id=\"onlineDeliveryAccept\"]/label/span[1]";
                string continueButtonXPath = "//*[@id=\"ssn-agree-modal-confirm-button\"]";
                string creditAccountXPath = "//*[@id=\"creditAccounts-section-link\"]/i";
                string creditCollectionAndBankruptcyXPath = "//*[@id=\"collectionsAndBankruptcy-section-link\"]";
                string creditCollectionXPath = "//*[@id=\"collections-and-bankruptcy-page-collections-section-link\"]";


                _elementLoader.Load(fileDisputeXPath, driver);
                _sleepLoader.Seconds(3);

                var element = driver.FindElement(By.XPath(checkboxXPath));
                element.Click();
                _sleepLoader.Seconds(3);

                _elementLoader.Load(continueButtonXPath, driver);
                _sleepLoader.Seconds(5);


                string creditor_name = string.Empty;
                string open_date = string.Empty;
                List<string> reasonArr = new List<string>();
                string comment = string.Empty;
                string issueOption = string.Empty;
                string reason = string.Empty;


                // Type of Request 🤔 Account / Collection / Inquiry ❓
                if (disputeRequest.equifax_data.account.Count > 0)
                {
                    // Iterating Account Data from Payload.
                    foreach (var account in disputeRequest.equifax_data.account)
                    {
                        if (!string.IsNullOrEmpty(account.creditor_name))
                        {
                            creditor_name = account.creditor_name;
                        }
                        if (!string.IsNullOrEmpty(account.open_date))
                        {
                            open_date = account.open_date;
                        }
                        if (account.accuracy.Count > 0)
                        {
                            reasonArr.AddRange(account.accuracy);
                        }
                        if (!string.IsNullOrEmpty(account.comment))
                        {
                            comment = account.comment;
                        }
                    }

                    _elementLoader.Load(creditAccountXPath, driver);
                    _sleepLoader.Seconds(3);


                    // Block Loader
                    blockElementsWithIndex = await _blockLoader.Process(driver);


                    IWebDriver driver_matchedBlock = await _blockMatchingLoader.Matching(blockElementsWithIndex, creditor_name, open_date, driver);


                    if (driver_matchedBlock != null)
                    {

                        string DisputeInProcessXPath = "//*[@id=\"wrapper\"]/sd-home/div/div/main/dispute-center/div/credit-account-details-page/div[5]/div/div/div";
                        string FileADisputeButtonXPath = "//*[@id=\"credit-account-details-page-dispute-information-btn\"]";
                        string continueXPath = "//*[@id=\"dispute-nav-buttons-continue-button\"]";


                        // Already Dispute InProcess
                        try
                        {
                            string InProcesstext = driver.FindElement(By.XPath(DisputeInProcessXPath)).Text.Trim();

                            if (InProcesstext == "A new dispute cannot be filed because a dispute is in progress or under investigation for this account.")
                            {
                                Console.WriteLine("Already Dispute InProcess");

                                var data = new
                                {
                                    file_number,
                                    comment = "REVIEW",
                                    submitted_date
                                };

                                response.status = true;
                                response.message = "Dispute Already Raised & InProcess, Please Wait Till Estimated Completion Time.";
                                response.data = data;

                                return response;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            throw ex;
                        }


                        //Procedding with New Dispute Request
                        Console.WriteLine("Procedding with New Dispute Request");

                        _elementLoader.Load(FileADisputeButtonXPath, driver);

                        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                        var radioButtons = driver.FindElements(By.Name("creditAccountIssue"));

                        if (radioButtons.Count >= 2)
                        {
                            js.ExecuteScript("arguments[0].click();", radioButtons[1]);
                        }
                        else
                        {
                            Console.WriteLine("Issue with Radio Button Load.");
                        }

                        _elementLoader.Load(continueXPath, driver);
                    }
                    else
                    {
                        Console.WriteLine("Sorry, No Matching Account Found as per your Request.");

                        var data = new
                        {
                            file_number,
                            comment = "Error : Something went wrong. Please try agian.",
                            submitted_date
                        };

                        response.status = true;
                        response.message = "Sorry, No Matching Account Found as per your Request.";
                        response.data = data;

                        return response;
                    }


                    var checkBoxArr = new List<string>
                    {
                        "//*[@id=\"dispute-checkbox-group-field-007\"]", // For 'The status, payment history, or payment rating...'
                        "//*[@id=\"dispute-checkbox-group-field-012\"]", // For 'I paid this account before it was closed...'
                        "//*[@id=\"dispute-checkbox-group-field-013\"]", // For 'The balance or past due amount is not correct.'
                        "//*[@id=\"dispute-checkbox-group-field-015\"]", // For 'The credit limit or high credit amount is inaccurate.'
                        "//*[@id=\"dispute-checkbox-group-field-016\"]", // For 'The date of first delinquency is inaccurate.'
                        "//*[@id=\"dispute-checkbox-group-field-024\"]", // For 'I closed this account.'
                        "//*[@id=\"dispute-checkbox-group-field-028\"]", // For 'The comment from the lender/creditor is not correct.'
                        "//*[@id=\"dispute-checkbox-group-field-037\"]"  // For 'This account is included in my bankruptcy.'
                    };


                    foreach (var checkBoxXPath in checkBoxArr)
                    {
                        _checkboxLoader.CheckboxHandelling(checkBoxXPath, driver, reasonArr);
                    }


                    string commentXPath = "//*[@id=\"creditAccountInfoComment\"]";
                    string saveBtnXPath = "//*[@id=\"dispute-comment-save-button-2\"]";
                    string continueBtnXPath = "//*[@id=\"dispute-nav-buttons-continue-button\"]";
                    string skipUploadXPath = "//*[@id=\"dispute-nav-buttons-skip-button\"]";
                    string submitDisputeXPath = "//*[@id=\"dispute-review-finish-and-upload-button\"]";
                    string confirmationNumberXPath = "//*[@id=\"dispute-confirmation-cards-list-confirmation-number\"]/div";

                    _sleepLoader.Seconds(3);
                    IWebElement commentElement = driver.FindElement(By.XPath(commentXPath));
                    commentElement.Clear();
                    commentElement.SendKeys(comment);

                    _elementLoader.Load(saveBtnXPath, driver);
                    _elementLoader.Load(continueBtnXPath, driver);
                    _elementLoader.Load(skipUploadXPath, driver);

                    try
                    {
                        _elementLoader.Load(submitDisputeXPath, driver);

                        _sleepLoader.Seconds(15);

                        var CONFIRMATION_ELEMENT = driver.FindElement(By.XPath(confirmationNumberXPath));
                        file_number = CONFIRMATION_ELEMENT.Text;

                        // Re-Check Confirmation Number
                        if (string.IsNullOrEmpty(file_number))
                        {
                            CONFIRMATION_ELEMENT = driver.FindElement(By.XPath("//*[@id=\"dispute-confirmation-cards-list-confirmation-number\"]/div"));
                            file_number = CONFIRMATION_ELEMENT.Text;
                        }

                        Console.WriteLine($"file_number: {file_number}");

                        var data = new
                        {
                            file_number,
                            comment = "Success",
                            submitted_date = DateTime.Now
                        };


                        response.status = true;
                        response.message = "Dispute Raised Successfully.";
                        response.data = data;

                        return response;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        throw ex;
                    }
                }


                if (disputeRequest.equifax_data.collection.Count > 0)
                {
                    // Iterating Collection Data from Payload.
                    foreach (var collection in disputeRequest.equifax_data.collection)
                    {
                        if (!string.IsNullOrEmpty(collection.creditor_name))
                        {
                            creditor_name = collection.creditor_name;
                        }
                        if (!string.IsNullOrEmpty(collection.issueOption))
                        {
                            issueOption = collection.issueOption;
                        }
                        if (!string.IsNullOrEmpty(collection.accuracy))
                        {
                            reason = collection.accuracy;
                        }
                        if (!string.IsNullOrEmpty(collection.comment))
                        {
                            comment = collection.comment;
                        }
                    }

                    _elementLoader.Load(creditCollectionAndBankruptcyXPath, driver);
                    _sleepLoader.Seconds(3);

                    _elementLoader.Load(creditCollectionXPath, driver);
                    _sleepLoader.Seconds(3);

                    // Extract the number of Total Open Accounts blocks
                    int totalBlocks = ExtractBlockCount(driver, By.XPath("//*[@id=\"wrapper\"]/sd-home/div/div/main/dispute-center/div/collections-page/collection-cards-list/h3"), "Total Open Accounts");

                    for (int i = 0; i < totalBlocks; i++)
                    {
                        try
                        {
                            driver.FindElement(By.XPath($"//*[@id=\"collection-cards-list-button-{i}\"]")).Click();
                        }
                        catch (NoSuchElementException ex)
                        {
                            Console.WriteLine($"No element found with Dynamic ID {i}: {ex.Message}");
                        }
                    }

                    driver.FindElement(By.XPath("//*[@id=\"collection-details-page-dispute-information-btn\"]")).Click();

                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    var radioButtonIssue = driver.FindElements(By.Name("collectionIssue"));

                    if (radioButtonIssue.Count >= 1)
                    {
                        js.ExecuteScript("arguments[0].click();", radioButtonIssue[0]);
                    }
                    else
                    {
                        Console.WriteLine("Issue with Radio Button Load.");
                    }

                    driver.FindElement(By.XPath("//*[@id=\"dispute-nav-buttons-continue-button\"]")).Click();


                    try
                    {
                        var radioButtonReason = driver.FindElements(By.Name("//*[@id=\"collection-ownership-reason-page-radio-group-field\"]"));

                        if (radioButtonReason.Count >= 1)
                        {
                            js.ExecuteScript("arguments[0].click();", radioButtonReason[1]);
                        }
                        else
                        {
                            Console.WriteLine("Issue with Radio Button Load.");
                        }
                    }
                    catch (Exception ex)
                    {
                        var radioButtonReason = driver.FindElements(By.Name("collectionOwnershipReasons"));

                        if (radioButtonReason.Count >= 1)
                        {
                            js.ExecuteScript("arguments[0].click();", radioButtonReason[1]);
                        }
                        else
                        {
                            Console.WriteLine("Issue with Radio Button Load.");
                        }

                        Console.WriteLine("");
                    }


                    var commentElement = driver.FindElement(By.XPath("//*[@id=\"collectionInfoReasonComment\"]"));
                    commentElement.Clear();
                    commentElement.SendKeys(comment);

                    driver.FindElement(By.XPath("//*[@id=\"dispute-comment-save-button-2\"]")).Click();
                    driver.FindElement(By.XPath("//*[@id=\"dispute-nav-buttons-continue-button\"]")).Click();
                    driver.FindElement(By.XPath("//*[@id=\"dispute-nav-buttons-skip-button\"]")).Click();
                    //driver.FindElement(By.XPath("//*[@id=\"dispute-review-finish-and-upload-button\"]")).Click();
                }


                if (disputeRequest.equifax_data.inquiries.Count > 0)
                {
                    foreach (var inquiry in disputeRequest.equifax_data.inquiries)
                    {
                        // Process Inquiry data here
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Filing Dispute Error: {ex.Message}");
                throw ex;
            }

            return response;
        }

        //-------------------HELPER FUNCTION--------------------//
        private int ExtractBlockCount(IWebDriver driver, By elementBy, string blockType)
        {
            _sleepLoader.Seconds(5);

            int blockCount = 0;

            try
            {
                // Locate the Block Text
                var blockElement = driver.FindElement(elementBy);
                string blockText = blockElement.GetAttribute("innerText");
                Console.WriteLine($"{blockType} Text: " + blockText);

                // Use Regex to extract the number inside parentheses
                var match = Regex.Match(blockText, @"\((\d+)\)");


                if (match.Success)
                {
                    if (int.TryParse(match.Groups[1].Value, out int parsedBlockCount))
                    {
                        blockCount = parsedBlockCount;
                    }
                    else
                    {
                        Console.WriteLine($"Failed to parse the number of {blockType} from text: {blockText}");
                    }
                }
                else
                {
                    Console.WriteLine($"Regex match failed. Could not extract {blockType} count from text: {blockText}");
                }
            }
            catch (NoSuchElementException ex)
            {
                Console.WriteLine($"No element found for {blockType}: {ex.Message}");
                throw ex;
            }

            // Return the extracted block count or 0 if not found
            return blockCount;
        }
    }
}
