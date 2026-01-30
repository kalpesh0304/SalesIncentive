using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.Employees.DTOs;
using MediatR;

namespace Dorise.Incentive.Application.Employees.Queries.GetEmployeeById;

/// <summary>
/// Query to get employee by their unique code.
/// </summary>
public record GetEmployeeByCodeQuery(string Code) : IRequest<EmployeeDto?>;

/// <summary>
/// Handler for GetEmployeeByCodeQuery.
/// </summary>
public class GetEmployeeByCodeQueryHandler : IRequestHandler<GetEmployeeByCodeQuery, EmployeeDto?>
{
    private readonly IReadOnlyRepository _repository;
    private readonly AutoMapper.IMapper _mapper;

    public GetEmployeeByCodeQueryHandler(IReadOnlyRepository repository, AutoMapper.IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<EmployeeDto?> Handle(GetEmployeeByCodeQuery request, CancellationToken cancellationToken)
    {
        var employee = await _repository.Employees.GetByCodeAsync(request.Code, cancellationToken);
        return employee == null ? null : _mapper.Map<EmployeeDto>(employee);
    }
}
