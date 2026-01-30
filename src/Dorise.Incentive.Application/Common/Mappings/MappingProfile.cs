using AutoMapper;
using Dorise.Incentive.Application.Calculations.DTOs;
using Dorise.Incentive.Application.Employees.DTOs;
using Dorise.Incentive.Application.IncentivePlans.DTOs;
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;

namespace Dorise.Incentive.Application.Common.Mappings;

/// <summary>
/// AutoMapper profile for mapping domain entities to DTOs.
/// "Can you open my milk, Mommy?" - AutoMapper opens the data for you!
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Employee mappings
        CreateMap<Employee, EmployeeDto>()
            .ForMember(d => d.EmployeeCode, opt => opt.MapFrom(s => s.EmployeeCode.Value))
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.FullName))
            .ForMember(d => d.BaseSalary, opt => opt.MapFrom(s => s.BaseSalary.Amount))
            .ForMember(d => d.Currency, opt => opt.MapFrom(s => s.BaseSalary.Currency));

        CreateMap<Employee, EmployeeSummaryDto>()
            .ForMember(d => d.EmployeeCode, opt => opt.MapFrom(s => s.EmployeeCode.Value))
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.FullName));

        CreateMap<Employee, EmployeeWithPlansDto>()
            .ForMember(d => d.EmployeeCode, opt => opt.MapFrom(s => s.EmployeeCode.Value))
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.FullName))
            .ForMember(d => d.BaseSalary, opt => opt.MapFrom(s => s.BaseSalary.Amount))
            .ForMember(d => d.Currency, opt => opt.MapFrom(s => s.BaseSalary.Currency));

        CreateMap<PlanAssignment, PlanAssignmentDto>()
            .ForMember(d => d.EffectiveFrom, opt => opt.MapFrom(s => s.EffectivePeriod.StartDate))
            .ForMember(d => d.EffectiveTo, opt => opt.MapFrom(s => s.EffectivePeriod.EndDate))
            .ForMember(d => d.PlanCode, opt => opt.MapFrom(s => s.IncentivePlan != null ? s.IncentivePlan.Code : string.Empty))
            .ForMember(d => d.PlanName, opt => opt.MapFrom(s => s.IncentivePlan != null ? s.IncentivePlan.Name : string.Empty));

        // IncentivePlan mappings
        CreateMap<IncentivePlan, IncentivePlanDto>()
            .ForMember(d => d.EffectiveFrom, opt => opt.MapFrom(s => s.EffectivePeriod.StartDate))
            .ForMember(d => d.EffectiveTo, opt => opt.MapFrom(s => s.EffectivePeriod.EndDate))
            .ForMember(d => d.TargetValue, opt => opt.MapFrom(s => s.Target.TargetValue))
            .ForMember(d => d.MinimumThreshold, opt => opt.MapFrom(s => s.Target.MinimumThreshold))
            .ForMember(d => d.AchievementType, opt => opt.MapFrom(s => s.Target.AchievementType))
            .ForMember(d => d.MetricUnit, opt => opt.MapFrom(s => s.Target.MetricUnit))
            .ForMember(d => d.MaximumPayout, opt => opt.MapFrom(s => s.MaximumPayout != null ? s.MaximumPayout.Amount : (decimal?)null))
            .ForMember(d => d.MinimumPayout, opt => opt.MapFrom(s => s.MinimumPayout != null ? s.MinimumPayout.Amount : (decimal?)null))
            .ForMember(d => d.Currency, opt => opt.MapFrom(s => s.MaximumPayout != null ? s.MaximumPayout.Currency : "INR"))
            .ForMember(d => d.PlanTypeDisplay, opt => opt.MapFrom(s => GetEnumDisplay(s.PlanType)))
            .ForMember(d => d.StatusDisplay, opt => opt.MapFrom(s => GetEnumDisplay(s.Status)))
            .ForMember(d => d.FrequencyDisplay, opt => opt.MapFrom(s => GetEnumDisplay(s.Frequency)));

        CreateMap<IncentivePlan, IncentivePlanSummaryDto>()
            .ForMember(d => d.EffectiveFrom, opt => opt.MapFrom(s => s.EffectivePeriod.StartDate))
            .ForMember(d => d.EffectiveTo, opt => opt.MapFrom(s => s.EffectivePeriod.EndDate))
            .ForMember(d => d.AssignedEmployeesCount, opt => opt.MapFrom(s => s.Assignments.Count));

        CreateMap<Slab, SlabDto>();

        // Calculation mappings
        CreateMap<Calculation, CalculationDto>()
            .ForMember(d => d.PeriodStart, opt => opt.MapFrom(s => s.CalculationPeriod.StartDate))
            .ForMember(d => d.PeriodEnd, opt => opt.MapFrom(s => s.CalculationPeriod.EndDate))
            .ForMember(d => d.AchievementPercentage, opt => opt.MapFrom(s => s.AchievementPercentage.Value))
            .ForMember(d => d.BaseSalary, opt => opt.MapFrom(s => s.BaseSalary.Amount))
            .ForMember(d => d.GrossIncentive, opt => opt.MapFrom(s => s.GrossIncentive.Amount))
            .ForMember(d => d.NetIncentive, opt => opt.MapFrom(s => s.NetIncentive.Amount))
            .ForMember(d => d.Currency, opt => opt.MapFrom(s => s.BaseSalary.Currency))
            .ForMember(d => d.ProrataFactor, opt => opt.MapFrom(s => s.ProrataFactor != null ? s.ProrataFactor.Value : (decimal?)null))
            .ForMember(d => d.StatusDisplay, opt => opt.MapFrom(s => GetEnumDisplay(s.Status)))
            .ForMember(d => d.EmployeeCode, opt => opt.MapFrom(s => s.Employee != null ? s.Employee.EmployeeCode.Value : string.Empty))
            .ForMember(d => d.EmployeeName, opt => opt.MapFrom(s => s.Employee != null ? s.Employee.FullName : string.Empty))
            .ForMember(d => d.PlanCode, opt => opt.MapFrom(s => s.IncentivePlan != null ? s.IncentivePlan.Code : string.Empty))
            .ForMember(d => d.PlanName, opt => opt.MapFrom(s => s.IncentivePlan != null ? s.IncentivePlan.Name : string.Empty));

        CreateMap<Calculation, CalculationSummaryDto>()
            .ForMember(d => d.PeriodStart, opt => opt.MapFrom(s => s.CalculationPeriod.StartDate))
            .ForMember(d => d.PeriodEnd, opt => opt.MapFrom(s => s.CalculationPeriod.EndDate))
            .ForMember(d => d.AchievementPercentage, opt => opt.MapFrom(s => s.AchievementPercentage.Value))
            .ForMember(d => d.NetIncentive, opt => opt.MapFrom(s => s.NetIncentive.Amount))
            .ForMember(d => d.Currency, opt => opt.MapFrom(s => s.BaseSalary.Currency))
            .ForMember(d => d.EmployeeCode, opt => opt.MapFrom(s => s.Employee != null ? s.Employee.EmployeeCode.Value : string.Empty))
            .ForMember(d => d.EmployeeName, opt => opt.MapFrom(s => s.Employee != null ? s.Employee.FullName : string.Empty))
            .ForMember(d => d.PlanName, opt => opt.MapFrom(s => s.IncentivePlan != null ? s.IncentivePlan.Name : string.Empty));

        CreateMap<Approval, ApprovalDto>()
            .ForMember(d => d.ApproverName, opt => opt.MapFrom(s => s.Approver != null ? s.Approver.FullName : string.Empty));
    }

    private static string GetEnumDisplay<T>(T value) where T : Enum
    {
        return value.ToString();
    }
}
