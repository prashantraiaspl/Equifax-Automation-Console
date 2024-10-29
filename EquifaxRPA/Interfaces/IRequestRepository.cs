using Equifax.Api.Domain.DTOs;
using Equifax.Api.Domain.Models;
using System.Threading.Tasks;

namespace Equifax.Api.Interfaces
{
    public interface IRequestRepository
    {
        Task<ResponseBody> CheckRequestQueueAsync(DisputeRequestDto requestDto);
        Task<ResponseBody> InsertRequestAsync(DisputeRequestDto requestDto);
        Task<ResponseBody> UpdateRequestAsync(RequestMaster response);
    }
}
