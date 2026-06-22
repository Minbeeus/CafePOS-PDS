using System.Threading.Tasks;
using CafePOS.Domain.Entities;

namespace CafePOS.Application.Interfaces.Repositories;

public interface IVoucherRepository
{
    Task<Voucher?> GetActiveByCodeAsync(string code);
    Task UpdateAsync(Voucher voucher);
}
