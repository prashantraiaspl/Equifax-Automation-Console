using Equifax.Api.AppDbContext;
using Equifax.Api.Domain.DTOs;
using Equifax.Api.Domain.Enums;
using Equifax.Api.Domain.Models;
using Equifax.Api.Interfaces;
using Equifax.Api.Repositories;
using Equifax.Api.Utilities;
using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chrome.ChromeDriverExtensions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EquifaxRPA.SERVICES
{
    public class Equifax : IEquifax
    {
        private readonly IRequestRepository _requestRepository;
        private readonly BrowserUtility _browserUtility;

        public Equifax()
        {
            var context = new ApplicationDbContext();
            _requestRepository = new RequestRepository(context);
            _browserUtility = new BrowserUtility();
        }


        public ResponseBody Test()
        {
            try
            {
                //path
                string driverPath = ConfigurationManager.AppSettings["driverPath"].ToString();
                //proxy configuration
                string strProxyUrl = ConfigurationManager.AppSettings["strProxyUrl"];
                string strPort = ConfigurationManager.AppSettings["strPort"];
                string strUsername = ConfigurationManager.AppSettings["strUsername"];
                string strPassword = ConfigurationManager.AppSettings["strPassword"];
                //chrome driver setup
                var chromeOptions = new ChromeOptions();
                chromeOptions.AddArgument("--start-maximized");
                chromeOptions.AddHttpProxy(strProxyUrl, Convert.ToInt32(strPort), strUsername, strPassword);
                var driver = new ChromeDriver(driverPath, chromeOptions);
                //delay
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5000)); // 5 sec delay
                //navigate
                driver.Navigate().GoToUrl("https://my.equifax.com");
                Thread.Sleep(10000);


                driver.Close();
                driver.Dispose();

                return new ResponseBody()
                {
                    data = null,
                    status = true,
                    message = "Equifax running on port : 7075"
                };
            }
            catch (Exception ex)
            {

                return new ResponseBody()
                {
                    data = null,
                    status = false,
                    message = ex.Message
                };
            }
            
        }


        public async Task<ResponseBody> Verify(DisputeRequestDto requestDto)
        {
            if (requestDto == null)
            {
                return new ResponseBody()
                {
                    status = false,
                    message = "Payload Neccessary Fields are Required."
                };
            }

            try
            {
                var loginCredentials = new LoginCredentialRequestDto
                {
                    user_name = requestDto.user_name,
                    user_password = requestDto.user_password
                };


                // Checking Existing Request in DB
                var requestData = await _requestRepository.CheckRequestQueueAsync(requestDto);


                if (requestData.data == null)
                {
                    // Handle new request
                    return await HandleNewRequestAsync(requestDto, loginCredentials);
                }
                else
                {
                    // Handle existing request
                    return await HandleExistingRequestAsync(requestData.data, requestDto, loginCredentials);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return new ResponseBody()
                {
                    status = false,
                    message = ex.Message
                };
            }
        }



        //---------------------------HELPER FUNCTIONS------------------------------//
        private async Task<ResponseBody> HandleNewRequestAsync(DisputeRequestDto requestDto, LoginCredentialRequestDto loginCredentials)
        {
            // Generate new request in DB
            await _requestRepository.InsertRequestAsync(requestDto);

            //_logger.LogInformation("New Request Generated and Inserted in DB");

            return await ProcessRequestWithBrowserAutomationAsync(requestDto, loginCredentials);
        }


        private async Task<ResponseBody> HandleExistingRequestAsync(IEnumerable<RequestMaster> requestData, DisputeRequestDto requestDto, LoginCredentialRequestDto loginCredentials)
        {
            foreach (var request in requestData)
            {
                if (request.request_status == "Completed")
                {
                    return BuildResponseBody(request, "Request has Already been Completed.");
                }
                else if (request.request_status == "Cancelled")
                {
                    return BuildResponseBody(request, "Request has been Cancelled.");
                }
            }

            // Continue last request if not completed or cancelled
            return await ProcessRequestWithBrowserAutomationAsync(requestDto, loginCredentials);
        }



        private async Task<ResponseBody> ProcessRequestWithBrowserAutomationAsync(DisputeRequestDto requestDto, LoginCredentialRequestDto loginCredentials)
        {
            //_logger.LogInformation($"Starting Browser Automation for URL: {_scrappingUrl}");
            string _scrappingUrl = ConfigurationManager.AppSettings["ScrappingURL"].ToString();

            Console.WriteLine($"Starting Browser Automation for URL: {_scrappingUrl}");


            ResponseBody result = await _browserUtility.BrowserAutomationProcess(_scrappingUrl, loginCredentials, requestDto);

            //_logger.LogError(result.message);

            if (result.status)
            {
                if (result.data.comment == "Success")
                {
                    //List<RequestMaster> updatedRequests = new List<RequestMaster>();

                    foreach (var account in requestDto.equifax_data.account)
                    {
                        var response = new RequestMaster
                        {
                            user_name = requestDto.user_name,
                            user_password = requestDto.user_password,
                            client_id = requestDto.client_id,
                            dispute_type = requestDto.dispute_type,
                            credit_repair_id = account.credit_repair_id,
                            creditor_name = account.creditor_name,
                            account_number = account.account_number,
                            credit_balance = account.credit_balance,
                            open_date = account.open_date,
                            creditor = account.creditor,
                            ownership = account.ownership,
                            accuracy = account.accuracy[0] + ", " + account.accuracy[1],
                            comment = result.data.comment,
                            file_number = result.data.file_number,
                            estimated_completion_date = account.estimated_completion_date,
                            submitted_date = result.data.submitted_date,
                            request_status = "Completed"
                        };

                        //var updateResult = await _requestRepository.UpdateRequest(response);
                        await _requestRepository.UpdateRequestAsync(response);
                    }
                }

                // Common API Logic for All Cases Error, Review, Success.
                var accountList = CreateAccountList(requestDto, result);
                var requestBody = CreateRequestBody(requestDto, accountList);

                var apiCallSuccess = await SendApiRequestAsync(requestBody);

                if (!apiCallSuccess && result.data.comment == "Success")
                {
                    return new ResponseBody
                    {
                        status = false,
                        message = "API Calling Failed."
                    };
                }

                return new ResponseBody
                {
                    status = apiCallSuccess,
                    message = apiCallSuccess ? "Dispute Request Processed Successfully." : "Failed to process dispute request.",
                    data = requestBody
                };
            }
            else
            {
                return new ResponseBody
                {
                    status = false,
                    message = "Something went wrong."
                };
            }
        }


        // Helper method to create account list for the request body
        private List<dynamic> CreateAccountList(DisputeRequestDto requestDto, ResponseBody result)
        {
            return requestDto.equifax_data.account.Select(account => new
            {
                credit_repair_id = account.credit_repair_id,
                creditor_name = account.creditor_name,
                account_number = account.account_number,
                credit_balance = account.credit_balance,
                open_date = account.open_date,
                comment = result.data.comment,
                file_number = result.data.file_number,
                estimated_completion_date = account.estimated_completion_date,
                submitted_date = result.data.submitted_date,
                creditor = account.creditor,
                ownership = account.ownership,
                accuracy = new[] { account.accuracy[0] + ", " + account.accuracy[1] }
            }).ToList<dynamic>();
        }


        // Helper method to create request body
        private object CreateRequestBody(DisputeRequestDto requestDto, List<object> accountList)
        {
            return new
            {
                user_name = requestDto.user_name,
                user_password = requestDto.user_password,
                client_id = requestDto.client_id,
                dispute_type = requestDto.dispute_type,
                equifax_data = new
                {
                    account = accountList,
                    collection = new object[] { },
                    inquiries = new object[] { }
                },
                transunion_data = new
                {
                    account = new object[] { },
                    collection = new object[] { },
                    inquiries = new object[] { }
                }
            };
        }


        // Helper method to send API request
        private async Task<bool> SendApiRequestAsync(object requestBody)
        {
            try
            {
                using (var httpClient = new HttpClient { BaseAddress = new Uri("https://crm.creditfreedomrestoration.com") })
                {
                    var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync("/vtiger_api.php?mode=SaveEquifaxResponse", jsonContent);

                    if (response.IsSuccessStatusCode)
                    {
                        var apiResponse = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(apiResponse);
                        return true;
                    }
                    else
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(errorResponse);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }


        private ResponseBody BuildResponseBody(RequestMaster request, string message)
        {
            return new ResponseBody
            {
                status = true,
                message = $"{message} Request ID: {request.RequestId}",
                data = request
            };
        }


    }
}
