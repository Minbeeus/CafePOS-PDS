using System;

namespace CafePOS.Infrastructure.Services;

public class VietQrService
{
    public string GenerateQrCodeUrl(string orderCode, decimal amount)
    {
        var bankId = "MB";
        var accountNo = "2302888888";
        var template = "compact2";
        var description = $"CAFEPOS {orderCode}";
        
        return $"https://img.vietqr.io/image/{bankId}-{accountNo}-{template}.png?amount={((int)amount)}&addInfo={Uri.EscapeDataString(description)}";
    }
}
