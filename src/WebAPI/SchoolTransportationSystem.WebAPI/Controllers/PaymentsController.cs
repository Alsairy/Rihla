using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SchoolTransportationSystem.Application.Interfaces;
using SchoolTransportationSystem.Application.DTOs;

namespace SchoolTransportationSystem.WebAPI.Controllers
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

        [HttpPost("secure-payment")]
        public async Task<ActionResult<SecurePaymentResultDto>> ProcessSecurePayment([FromBody] SecurePaymentRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tenantId = "1";
                var result = await _paymentService.ProcessSecurePaymentAsync(request, tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error processing secure payment", error = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error processing secure payment", error = ex.Message });
            }
        }

        [HttpPost("generate-invoices")]
        public async Task<ActionResult<IEnumerable<InvoiceDto>>> GenerateAutomatedInvoices([FromBody] InvoiceGenerationRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tenantId = "1";
                var result = await _paymentService.GenerateAutomatedInvoicesAsync(request, tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error generating automated invoices", error = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating automated invoices", error = ex.Message });
            }
        }

        [HttpPost("apply-family-discounts")]
        public async Task<ActionResult<FamilyDiscountResultDto>> ApplyFamilyDiscounts([FromBody] FamilyDiscountRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tenantId = "1";
                var result = await _paymentService.ApplyFamilyDiscountsAsync(request, tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error applying family discounts", error = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error applying family discounts", error = ex.Message });
            }
        }

        [HttpPost("{id}/refund")]
        public async Task<ActionResult<RefundResultDto>> ProcessRefund(int id, [FromBody] RefundRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tenantId = "1";
                var result = await _paymentService.ProcessRefundsAsync(id, request, tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error processing refund", error = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error processing refund", error = ex.Message });
            }
        }

        [HttpGet("gateway-status")]
        public async Task<ActionResult<PaymentGatewayStatusDto>> GetPaymentGatewayStatus()
        {
            try
            {
                var tenantId = "1";
                var result = await _paymentService.GetPaymentGatewayStatusAsync(tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error retrieving payment gateway status", error = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving payment gateway status", error = ex.Message });
            }
        }

        [HttpGet("fraud-detection")]
        public async Task<ActionResult<FraudDetectionResultDto>> GetFraudDetectionResults([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var tenantId = "1";
                var result = await _paymentService.GetFraudDetectionResultsAsync(startDate, endDate, tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error retrieving fraud detection results", error = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving fraud detection results", error = ex.Message });
            }
        }

        [HttpGet("payment-analytics")]
        public async Task<ActionResult<PaymentAnalyticsDto>> GetPaymentAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var tenantId = "1";
                var result = await _paymentService.GetPaymentAnalyticsAsync(startDate, endDate, tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error retrieving payment analytics", error = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving payment analytics", error = ex.Message });
            }
        }
    }
}

