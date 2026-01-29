using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;

namespace Dorise.Incentive.Application.Employees.Commands.DeleteEmployee;

/// <summary>
/// Handler for DeleteEmployeeCommand.
/// Performs a soft delete by changing status to Inactive.
/// </summary>
public class DeleteEmployeeCommandHandler : ICommandHandler<DeleteEmployeeCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteEmployeeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(request.Id, cancellationToken);

        if (employee == null)
        {
            return Result.NotFound("Employee", request.Id);
        }

        // Soft delete - change status to Inactive
        employee.ChangeStatus(EmployeeStatus.Inactive);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
