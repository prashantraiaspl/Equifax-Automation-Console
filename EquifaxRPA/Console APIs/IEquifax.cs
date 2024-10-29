using Equifax.Api.Domain.DTOs;
using EquifaxRPA;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;

namespace EquifaxRPA.SERVICES
{
    [ServiceContract(Namespace = "Equifax Automation")]
    public interface IEquifax
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        ResponseBody Test();

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        Task<ResponseBody> Verify(DisputeRequestDto requestDto);
    }
}
