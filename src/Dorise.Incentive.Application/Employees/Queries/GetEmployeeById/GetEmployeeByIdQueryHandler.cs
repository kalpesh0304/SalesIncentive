using AutoMapper;
using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.Employees.DTOs;
using Dorise.Incentive.Domain.Interfaces;

namespace Dorise.Incentive.Application.Employees.Queries.GetEmployeeById;

/// <summary>
/// Handler for GetEmployeeByIdQuery.
/// </summary>
public class GetEmployeeByIdQueryHandler : IQueryHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetEmployeeByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<EmployeeDto>> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(request.EmployeeId, cancellationToken);

        if (employee == null)
        {
            return Result.NotFound<EmployeeDto>("Employee", request.EmployeeId);
        }

        var dto = _mapper.Map<EmployeeDto>(employee);
        return Result.Success(dto);
    }
}
