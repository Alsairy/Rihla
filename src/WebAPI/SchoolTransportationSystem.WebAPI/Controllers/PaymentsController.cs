using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Rihla.Application.Interfaces;
using Rihla.Application.DTOs;

namespace Rihla.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPayments([FromQuery] PaymentSearchDto searchDto)
        {
            try
            {
                var tenantId = "1";
                var result = await _paymentService.GetAllAsync(searchDto ?? new PaymentSearchDto(), tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error retrieving payments", error = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving payments", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDto>> GetPayment(int id)
        {
            try
            {
                var tenantId = "1";
                var result = await _paymentService.GetByIdAsync(id, tenantId);
                
                if (!result.IsSuccess)
                    return NotFound(new { message = $"Payment with ID {id} not found" });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving payment", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<PaymentDto>> CreatePayment([FromBody] CreatePaymentDto createPaymentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tenantId = "1";
                createPaymentDto.TenantId = tenantId;
                var result = await _paymentService.CreateAsync(createPaymentDto, tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error creating payment", error = result.Error });

                return CreatedAtAction(nameof(GetPayment), new { id = result.Value.Id }, result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating payment", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] UpdatePaymentDto updatePaymentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tenantId = "1";
                updatePaymentDto.Id = id;
                var result = await _paymentService.UpdateAsync(id, updatePaymentDto, tenantId);
                
                if (!result.IsSuccess)
                    return NotFound(new { message = $"Payment with ID {id} not found" });

                return Ok(new { message = "Payment updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating payment", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            try
            {
                var tenantId = "1";
                var result = await _paymentService.DeleteAsync(id, tenantId);
                
                if (!result.IsSuccess)
                    return NotFound(new { message = $"Payment with ID {id} not found" });

                return Ok(new { message = "Payment deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting payment", error = ex.Message });
            }
        }

        [HttpGet("statistics")]
        public async Task<ActionResult> GetPaymentStatistics()
        {
            try
            {
                var stats = new
                {
                    TotalRevenue = 0.0,
                    MonthlyRevenue = 0.0,
                    PendingPayments = 0,
                    CompletedPayments = 0,
                    SuccessRate = 0.0
                };
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving payment statistics", error = ex.Message });
            }
        }
    }
}

