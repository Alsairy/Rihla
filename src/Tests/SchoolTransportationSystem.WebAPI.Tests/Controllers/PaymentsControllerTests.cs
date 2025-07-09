using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SchoolTransportationSystem.WebAPI.Controllers;
using SchoolTransportationSystem.Application.Interfaces;
using SchoolTransportationSystem.Application.DTOs;
using SchoolTransportationSystem.Core.Common;
using SchoolTransportationSystem.Core.Enums;

namespace SchoolTransportationSystem.WebAPI.Tests.Controllers
{
    public class PaymentsControllerTests
    {
        private readonly Mock<IPaymentService> _mockPaymentService;
        private readonly PaymentsController _controller;
        private const string TestTenantId = "1";

        public PaymentsControllerTests()
        {
            _mockPaymentService = new Mock<IPaymentService>();
            _controller = new PaymentsController(_mockPaymentService.Object);
        }

        [Fact]
        public async Task GetPayments_ReturnsOkResult_WhenPaymentsExist()
        {
            var searchDto = new PaymentSearchDto();
            var payments = new List<PaymentDto>
            {
                new PaymentDto { Id = 1, StudentId = 1, Amount = 500.00m, Method = PaymentMethod.CreditCard },
                new PaymentDto { Id = 2, StudentId = 2, Amount = 750.00m, Method = PaymentMethod.Cash }
            };
            var pagedResult = new PagedResult<PaymentDto>
            {
                Items = payments,
                TotalCount = 2,
                Page = 1,
                PageSize = 20
            };

            _mockPaymentService
                .Setup(s => s.GetAllAsync(It.IsAny<PaymentSearchDto>(), TestTenantId))
                .ReturnsAsync(Result<PagedResult<PaymentDto>>.Success(pagedResult));

            var result = await _controller.GetPayments(searchDto);

            var actionResult = result.Should().BeOfType<ActionResult<IEnumerable<PaymentDto>>>().Subject;
            var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedData = okResult.Value.Should().BeAssignableTo<PagedResult<PaymentDto>>().Subject;
            returnedData.Items.Should().HaveCount(2);
            returnedData.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetPayment_ReturnsOkResult_WhenPaymentExists()
        {
            var paymentId = 1;
            var payment = new PaymentDto 
            { 
                Id = paymentId, 
                StudentId = 1, 
                Amount = 500.00m, 
                Method = PaymentMethod.CreditCard 
            };

            _mockPaymentService
                .Setup(s => s.GetByIdAsync(paymentId, TestTenantId))
                .ReturnsAsync(Result<PaymentDto>.Success(payment));

            var result = await _controller.GetPayment(paymentId);

            var actionResult = result.Should().BeOfType<ActionResult<PaymentDto>>().Subject;
            var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedData = okResult.Value.Should().BeAssignableTo<PaymentDto>().Subject;
            returnedData.Id.Should().Be(paymentId);
            returnedData.Amount.Should().Be(500.00m);
        }

        [Fact]
        public async Task CreatePayment_ReturnsCreatedResult_WhenValidPayment()
        {
            var createDto = new CreatePaymentDto
            {
                StudentId = 1,
                Amount = 500.00m,
                Method = PaymentMethod.CreditCard,
                Type = PaymentType.Recurring,
                DueDate = DateTime.UtcNow.AddDays(30),
                Description = "Monthly transportation fee",
                TenantId = TestTenantId
            };

            var createdPayment = new PaymentDto
            {
                Id = 1,
                StudentId = createDto.StudentId,
                Amount = createDto.Amount,
                Method = createDto.Method
            };

            _mockPaymentService
                .Setup(s => s.CreateAsync(createDto, TestTenantId))
                .ReturnsAsync(Result<PaymentDto>.Success(createdPayment));

            var result = await _controller.CreatePayment(createDto);

            var actionResult = result.Should().BeOfType<ActionResult<PaymentDto>>().Subject;
            var createdResult = actionResult.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var returnedData = createdResult.Value.Should().BeAssignableTo<PaymentDto>().Subject;
            returnedData.Id.Should().Be(1);
            returnedData.Amount.Should().Be(500.00m);
        }

        [Fact]
        public async Task UpdatePayment_ReturnsOkResult_WhenValidUpdate()
        {
            var paymentId = 1;
            var updateDto = new UpdatePaymentDto
            {
                Id = paymentId,
                Amount = 600.00m,
                Method = PaymentMethod.BankTransfer,
                Type = PaymentType.Recurring,
                DueDate = DateTime.UtcNow.AddDays(30),
                Description = "Updated transportation fee"
            };

            var updatedPayment = new PaymentDto
            {
                Id = paymentId,
                StudentId = 1,
                Amount = updateDto.Amount,
                Method = updateDto.Method,
                Type = updateDto.Type,
                DueDate = updateDto.DueDate,
                Description = updateDto.Description
            };

            _mockPaymentService
                .Setup(s => s.UpdateAsync(paymentId, updateDto, TestTenantId))
                .ReturnsAsync(Result<PaymentDto>.Success(updatedPayment));

            var result = await _controller.UpdatePayment(paymentId, updateDto);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task DeletePayment_ReturnsOkResult_WhenPaymentExists()
        {
            var paymentId = 1;
            _mockPaymentService
                .Setup(s => s.DeleteAsync(paymentId, TestTenantId))
                .ReturnsAsync(Result<bool>.Success(true));

            var result = await _controller.DeletePayment(paymentId);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
        }
    }
}
