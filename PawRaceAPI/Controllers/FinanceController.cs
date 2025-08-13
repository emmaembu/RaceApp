using Microsoft.AspNetCore.Mvc;
using Wallet;

namespace OfferAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinanceController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public FinanceController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpPost("ensure-wallet/{userId}")]
        public IActionResult EnsureWallet(Guid user)
        {
             _walletService.CheckIfWalletExists(user);
            return Ok();
        }

        [HttpGet("balance/{userId}")]
        public IActionResult GetBalance(Guid user)
        {
            var balance =  _walletService.GetBalance(user);
            return Ok(balance); 
        }

        [HttpPost("deposit")]
        public IActionResult Deposit(Guid user, decimal amount)
        {
            _walletService.Deposit(user, amount);   
            return Ok();
        }

        [HttpPost("withdraw")]
        public IActionResult Withdraw(Guid user, decimal amount)
        {
            var success = _walletService.TryWithdraw(user, amount);

            return success ? Ok() : BadRequest();
        }
    }
}
