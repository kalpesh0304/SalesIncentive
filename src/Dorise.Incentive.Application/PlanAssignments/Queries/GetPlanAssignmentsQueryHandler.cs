using AutoMapper;
using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.PlanAssignments.DTOs;
using MediatR;

namespace Dorise.Incentive.Application.PlanAssignments.Queries;

/// <summary>
/// Handler for GetPlanAssignmentsQuery.
/// "I eated the purple berries!" - And we fetched all the assignments!
/// </summary>
public class GetPlanAssignmentsQueryHandler : IRequestHandler<GetPlanAssignmentsQuery, IReadOnlyList<PlanAssignmentDto>>
{
    private readonly IReadOnlyRepository _repository;
    private readonly IMapper _mapper;

    public GetPlanAssignmentsQueryHandler(IReadOnlyRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<PlanAssignmentDto>> Handle(
        GetPlanAssignmentsQuery request,
        CancellationToken cancellationToken)
    {
        var assignments = await _repository.PlanAssignments.GetFilteredAsync(
            request.EmployeeId,
            request.PlanId,
            request.ActiveOnly,
            cancellationToken);

        return _mapper.Map<IReadOnlyList<PlanAssignmentDto>>(assignments);
    }
}

/// <summary>
/// Handler for GetPlanAssignmentByIdQuery.
/// </summary>
public class GetPlanAssignmentByIdQueryHandler : IRequestHandler<GetPlanAssignmentByIdQuery, PlanAssignmentDto?>
{
    private readonly IReadOnlyRepository _repository;
    private readonly IMapper _mapper;

    public GetPlanAssignmentByIdQueryHandler(IReadOnlyRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PlanAssignmentDto?> Handle(
        GetPlanAssignmentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var assignment = await _repository.PlanAssignments.GetWithDetailsAsync(request.Id, cancellationToken);
        return assignment == null ? null : _mapper.Map<PlanAssignmentDto>(assignment);
    }
}

/// <summary>
/// Handler for GetEmployeeAssignmentsQuery.
/// </summary>
public class GetEmployeeAssignmentsQueryHandler : IRequestHandler<GetEmployeeAssignmentsQuery, IReadOnlyList<PlanAssignmentDto>>
{
    private readonly IReadOnlyRepository _repository;
    private readonly IMapper _mapper;

    public GetEmployeeAssignmentsQueryHandler(IReadOnlyRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<PlanAssignmentDto>> Handle(
        GetEmployeeAssignmentsQuery request,
        CancellationToken cancellationToken)
    {
        var assignments = await _repository.PlanAssignments.GetByEmployeeAsync(
            request.EmployeeId,
            request.ActiveOnly,
            cancellationToken);

        return _mapper.Map<IReadOnlyList<PlanAssignmentDto>>(assignments);
    }
}
