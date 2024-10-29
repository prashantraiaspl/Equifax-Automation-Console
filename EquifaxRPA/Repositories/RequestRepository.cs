using Equifax.Api.AppDbContext;
using Equifax.Api.Domain.DTOs;
using Equifax.Api.Domain.Enums;
using Equifax.Api.Domain.Models;
using Equifax.Api.Interfaces;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace Equifax.Api.Repositories
{
    public class RequestRepository : IRequestRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<RequestMaster> _dbSet;

        public RequestRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<RequestMaster>();
        }


        // Checking the Existing Request from Database.
        public async Task<ResponseBody> CheckRequestQueueAsync(DisputeRequestDto requestDto)
        {
            var responseBody = new ResponseBody();

            try
            {
                var query = _dbSet
                    .Where(request => request.request_status == "InProgress" && request.client_id == requestDto.client_id);


                var result = await query.ToListAsync();


                if (result.Count == 0)
                {
                    responseBody.status = false;
                    responseBody.message = "Record Not Found.";
                }
                else
                {
                    responseBody.status = true;
                    responseBody.message = "Record Found Successfully.";
                    responseBody.data = result;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                responseBody.status = false;
                responseBody.message = ex.Message;
            }

            return responseBody;
        }


        // Inserting the New Request in Database.
        public async Task<ResponseBody> InsertRequestAsync(DisputeRequestDto requestDto)
        {
            var responseBody = new ResponseBody();

            try
            {
                var newRequest = new RequestMaster
                {
                    user_name = requestDto.user_name,
                    user_password = requestDto.user_password,
                    client_id = requestDto.client_id,
                    dispute_type = requestDto.dispute_type,
                    request_status = "InProgress",
                    CreatedAt = DateTime.Now,
                };

                var result = _dbSet.Add(newRequest);

                await _context.SaveChangesAsync();

                responseBody.status = true;
                responseBody.message = "Request Inserted Successfully.";
                responseBody.data = result;
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine(dbEx.InnerException?.Message ?? dbEx.Message);
                responseBody.status = false;
                responseBody.message = "Database update failed. Please check the inner exception for more details.";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                responseBody.status = false;
                responseBody.message = ex.Message;
            }

            return responseBody;
        }


        // Updating the Request in Database.
        public async Task<ResponseBody> UpdateRequestAsync(RequestMaster requestdto)
        {
            var responseBody = new ResponseBody();

            try
            {
                var existingRequest = await _dbSet.FirstOrDefaultAsync(request =>
                    request.client_id == requestdto.client_id &&
                    request.request_status == "InProgress");


                if (existingRequest == null)
                {
                    responseBody.status = false;
                    responseBody.message = "Sorry, Request Not Found.";
                    return responseBody;
                }

                else
                {
                    existingRequest.credit_repair_id = requestdto.credit_repair_id;
                    existingRequest.creditor_name = requestdto.creditor_name;
                    existingRequest.account_number = requestdto.account_number;
                    existingRequest.credit_balance = requestdto.credit_balance;
                    existingRequest.open_date = requestdto.open_date;
                    existingRequest.creditor = requestdto.creditor;
                    existingRequest.ownership = requestdto.ownership;
                    existingRequest.accuracy = requestdto.accuracy;
                    existingRequest.comment = requestdto.comment;
                    existingRequest.file_number = requestdto.file_number;
                    existingRequest.estimated_completion_date = requestdto.estimated_completion_date;
                    existingRequest.submitted_date = requestdto.submitted_date;
                    existingRequest.request_status = requestdto.request_status;
                    existingRequest.UpdatedAt = DateTime.Now;

                    await _context.SaveChangesAsync();
                }

                var response = await _dbSet.FirstOrDefaultAsync(request => request.RequestId == existingRequest.RequestId);

                responseBody.status = true;
                responseBody.message = "Request Updated Successfully.";
                responseBody.data = response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                responseBody.status = false;
                responseBody.message = ex.Message;
            }

            return responseBody;
        }
    }
}
