using CoachOnline.Model;
using CoachOnline.Model.ApiRequests.Admin;
using CoachOnline.Model.ApiResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Interfaces
{
    public interface IContract
    {
        Task<int> AddContract(AddContractRqs rqs);
        Task DeleteContract(int contractId);
        Task UpdateContract(int contractId, UpdateContractRqs rqs);
        Task<ContractResponse> GetLatestContract(ContractType cType);
        Task<List<ContractResponseAdmin>> GetContracts(ContractType cType);
    }

 
}
