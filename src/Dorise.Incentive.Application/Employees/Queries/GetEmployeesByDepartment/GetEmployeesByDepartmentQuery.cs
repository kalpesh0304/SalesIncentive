using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.Employees.DTOs;
using MediatR;

namespace Dorise.Incentive.Application.Employees.Queries.GetEmployees;

/// <summary>
/// Query to get employees by department.
/// "I like men now!" - Departments like having employees!
/// </summary>
public record GetEmployeesByDepartmentQuery(Guid DepartmentId) : IRequest<IReadOnlyList<EmployeeSummaryDto>>;

/// <summary>
/// Handler for GetEmployeesByDepartmentQuery.
/// </summary>
public class GetEmployeesByDepartmentQueryHandler : IRequestHandler<GetEmployeesByDepartmentQuery, IReadOnlyList<EmployeeSummaryDto>>
{
    private readonly IReadOnlyRepository _repository;
    private readonly AutoMapper.IMapper _mapper;

    public GetEmployeesByDepartmentQueryHandler(IReadOnlyRepository repository, AutoMapper.IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<EmployeeSummaryDto>> Handle(
        GetEmployeesByDepartmentQuery request,
        CancellationToken cancellationToken)
    {
        var employees = await _repository.Employees.GetByDepartmentAsync(request.DepartmentId, cancellationToken: cancellationToken);
        return _mapper.Map<IReadOnlyList<EmployeeSummaryDto>>(employees);
    }
}
