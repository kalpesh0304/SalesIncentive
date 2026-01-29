using AutoMapper;
using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.IncentivePlans.DTOs;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Services;
using MediatR;

namespace Dorise.Incentive.Application.IncentivePlans.Queries;

/// <summary>
/// Handler for GetIncentivePlansQuery.
/// </summary>
public class GetIncentivePlansQueryHandler : IRequestHandler<GetIncentivePlansQuery, PagedResult<IncentivePlanSummaryDto>>
{
    private readonly IReadOnlyRepository _repository;
    private readonly IMapper _mapper;

    public GetIncentivePlansQueryHandler(IReadOnlyRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<IncentivePlanSummaryDto>> Handle(
        GetIncentivePlansQuery request,
        CancellationToken cancellationToken)
    {
        PlanStatus? status = null;
        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<PlanStatus>(request.Status, true, out var parsedStatus))
        {
            status = parsedStatus;
        }

        PlanType? planType = null;
        if (!string.IsNullOrEmpty(request.PlanType) && Enum.TryParse<PlanType>(request.PlanType, true, out var parsedType))
        {
            planType = parsedType;
        }

        var (plans, totalCount) = await _repository.IncentivePlans.GetPagedAsync(
            request.Page,
            request.PageSize,
            status,
            planType,
            request.Search,
            cancellationToken);

        return new PagedResult<IncentivePlanSummaryDto>
        {
            Items = _mapper.Map<IReadOnlyList<IncentivePlanSummaryDto>>(plans),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}

/// <summary>
/// Handler for GetIncentivePlanByIdQuery.
/// </summary>
public class GetIncentivePlanByIdQueryHandler : IRequestHandler<GetIncentivePlanByIdQuery, IncentivePlanDto?>
{
    private readonly IReadOnlyRepository _repository;
    private readonly IMapper _mapper;

    public GetIncentivePlanByIdQueryHandler(IReadOnlyRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IncentivePlanDto?> Handle(
        GetIncentivePlanByIdQuery request,
        CancellationToken cancellationToken)
    {
        var plan = await _repository.IncentivePlans.GetByIdAsync(request.Id, cancellationToken);
        return plan == null ? null : _mapper.Map<IncentivePlanDto>(plan);
    }
}

/// <summary>
/// Handler for GetIncentivePlanByCodeQuery.
/// </summary>
public class GetIncentivePlanByCodeQueryHandler : IRequestHandler<GetIncentivePlanByCodeQuery, IncentivePlanDto?>
{
    private readonly IReadOnlyRepository _repository;
    private readonly IMapper _mapper;

    public GetIncentivePlanByCodeQueryHandler(IReadOnlyRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IncentivePlanDto?> Handle(
        GetIncentivePlanByCodeQuery request,
        CancellationToken cancellationToken)
    {
        var plan = await _repository.IncentivePlans.GetByCodeAsync(request.Code, cancellationToken);
        return plan == null ? null : _mapper.Map<IncentivePlanDto>(plan);
    }
}

/// <summary>
/// Handler for GetIncentivePlanWithSlabsQuery.
/// </summary>
public class GetIncentivePlanWithSlabsQueryHandler : IRequestHandler<GetIncentivePlanWithSlabsQuery, IncentivePlanWithSlabsDto?>
{
    private readonly IReadOnlyRepository _repository;
    private readonly IMapper _mapper;

    public GetIncentivePlanWithSlabsQueryHandler(IReadOnlyRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IncentivePlanWithSlabsDto?> Handle(
        GetIncentivePlanWithSlabsQuery request,
        CancellationToken cancellationToken)
    {
        var plan = await _repository.IncentivePlans.GetWithSlabsAsync(request.Id, cancellationToken);
        return plan == null ? null : _mapper.Map<IncentivePlanWithSlabsDto>(plan);
    }
}

/// <summary>
/// Handler for GetActivePlansQuery.
/// </summary>
public class GetActivePlansQueryHandler : IRequestHandler<GetActivePlansQuery, IReadOnlyList<IncentivePlanSummaryDto>>
{
    private readonly IReadOnlyRepository _repository;
    private readonly IMapper _mapper;

    public GetActivePlansQueryHandler(IReadOnlyRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<IncentivePlanSummaryDto>> Handle(
        GetActivePlansQuery request,
        CancellationToken cancellationToken)
    {
        var plans = await _repository.IncentivePlans.GetActivePlansAsync(request.EffectiveDate, cancellationToken);
        return _mapper.Map<IReadOnlyList<IncentivePlanSummaryDto>>(plans);
    }
}

/// <summary>
/// Handler for ValidateIncentivePlanQuery.
/// </summary>
public class ValidateIncentivePlanQueryHandler : IRequestHandler<ValidateIncentivePlanQuery, PlanValidationResultDto?>
{
    private readonly IReadOnlyRepository _repository;
    private readonly IPlanValidationService _validationService;

    public ValidateIncentivePlanQueryHandler(
        IReadOnlyRepository repository,
        IPlanValidationService validationService)
    {
        _repository = repository;
        _validationService = validationService;
    }

    public async Task<PlanValidationResultDto?> Handle(
        ValidateIncentivePlanQuery request,
        CancellationToken cancellationToken)
    {
        var plan = await _repository.IncentivePlans.GetWithSlabsAsync(request.Id, cancellationToken);

        if (plan == null)
        {
            return null;
        }

        var result = _validationService.ValidatePlan(plan);

        return new PlanValidationResultDto
        {
            PlanId = plan.Id,
            IsValid = result.IsValid,
            CanBeActivated = result.CanBeActivated,
            Errors = result.Errors.Select(e => new ValidationIssueDto
            {
                Code = e.Code,
                Message = e.Message,
                Field = e.Field,
                Severity = "Error"
            }).ToList(),
            Warnings = result.Warnings.Select(w => new ValidationIssueDto
            {
                Code = w.Code,
                Message = w.Message,
                Field = w.Field,
                Severity = "Warning"
            }).ToList()
        };
    }
}
