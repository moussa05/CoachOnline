using CoachOnline.Helpers;
using CoachOnline.Implementation;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiRequests.Admin;
using CoachOnline.Model.ApiResponses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Services
{
    public class ContractsService: IContract
    {
        private readonly ILogger<ContractsService> _logger;
        public ContractsService(ILogger<ContractsService> logger)
        {
            _logger = logger;
        }

        public async Task<ContractResponse> GetLatestContract(ContractType cType)
        {
            var resp = new ContractResponse();
            using (var ctx = new DataContext())
            {
                var contracts = await ctx.Contracts.Where(c => c.Type == cType).ToListAsync();

                if(contracts.Any())
                {
                    var currentExists = contracts.Where(t => t.IsCurrent).OrderByDescending(t => t.LastUpdateDate).FirstOrDefault();

                    if(currentExists != null)
                    {
                        resp.ContractBody = currentExists.Body;
                        resp.ContractId = currentExists.Id;
                        resp.ContractName = currentExists.Name;
                        resp.Type = currentExists.Type;
                        return resp;
                    }
                    else
                    {
                        var other = contracts.OrderByDescending(t => t.LastUpdateDate).FirstOrDefault();
                        if (other != null)
                        {
                            resp.ContractBody = other.Body;
                            resp.ContractId = other.Id;
                            resp.ContractName = other.Name;
                            resp.Type = other.Type;
                            return resp;
                        }
                    }
                }

                return null;
            }
        }

        public async Task<List<ContractResponseAdmin>> GetContracts(ContractType cType)
        {
            var resp = new List<ContractResponseAdmin>();
            using (var ctx = new DataContext())
            {
                var contracts = await ctx.Contracts.Where(c => c.Type == cType).ToListAsync();

                if (contracts.Any())
                {
                    contracts.ForEach(c =>
                    {
                        var data = new ContractResponseAdmin();
                        data.ContractBody = c.Body;
                        data.ContractId = c.Id;
                        data.ContractName = c.Name;
                        data.Type = c.Type;
                        data.LastUpdateDate = c.LastUpdateDate;
                        data.CreationDate = c.CreationDate;
                        data.IsCurrent = c.IsCurrent;
                        

                        resp.Add(data);
                    });

                    return resp;
                }

                return null;
            }
        }

        public async Task<int> AddContract(AddContractRqs rqs)
        {
            using(var ctx = new DataContext())
            {
                var contract = new Contract();
                contract.Body = rqs.Body;
                contract.Name = rqs.Name;
                contract.CreationDate = DateTime.Now;
                contract.LastUpdateDate = DateTime.Now;
                contract.Type = rqs.Type;

                if(rqs.IsCurrent.HasValue)
                {
                    contract.IsCurrent = rqs.IsCurrent.Value;

                    if(rqs.IsCurrent.Value)
                    {
                        await UpdateOtherContractsToNotCurrentlyValid(rqs.Type);
                    }
                }
                else
                {
                    contract.IsCurrent = false;
                }
              

                ctx.Contracts.Add(contract);
                await ctx.SaveChangesAsync();

                return contract.Id;
            }
        }

        public async Task DeleteContract(int contractId)
        {
            using(var ctx = new DataContext())
            {
                var ctrct = await ctx.Contracts.FirstOrDefaultAsync(x=>x.Id == contractId);

                ctx.Contracts.Remove(ctrct);

                await ctx.SaveChangesAsync();
            }
        }

        public async Task UpdateContract(int contractId, UpdateContractRqs rqs)
        {
            using(var ctx = new DataContext())
            {
                var ctrct = await ctx.Contracts.FirstOrDefaultAsync(x => x.Id == contractId);
                ctrct.CheckExist("Contract");
               
                if (!string.IsNullOrEmpty(rqs.Body))
                {
                    ctrct.Body = rqs.Body;
                }
                if (!string.IsNullOrEmpty(rqs.Name))
                {
                    ctrct.Name = rqs.Name;
                }
                if (rqs.IsCurrent.HasValue)
                {
                    if(rqs.IsCurrent.Value)
                    {
                        await UpdateOtherContractsToNotCurrentlyValid(ctrct.Type, ctrct.Id);
                    }
                    ctrct.IsCurrent = rqs.IsCurrent.Value;
                }

                ctrct.LastUpdateDate = DateTime.Now;

                await ctx.SaveChangesAsync();
            }
        }

        private async Task UpdateOtherContractsToNotCurrentlyValid(ContractType cType, int? contractId = null)
        {
            using(var ctx = new DataContext())
            {
                List<Contract> contracts = null;

                if (!contractId.HasValue)
                {
                    contracts = await ctx.Contracts.Where(x => x.Type == cType && x.IsCurrent).ToListAsync();
                }
                else
                {
                    contracts = await ctx.Contracts.Where(x => x.Id != contractId && x.Type == cType && x.IsCurrent).ToListAsync();
                }

                foreach(var c in contracts)
                {
                    c.IsCurrent = false;
                    c.LastUpdateDate = DateTime.Now;
                }

                await ctx.SaveChangesAsync();
            }
        }
    }
}
