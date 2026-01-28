using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using ManufacturingOptimization.ProviderSimulator.Models;

namespace ManufacturingOptimization.ProviderSimulator.TechnologyProviders;

/// <summary>
/// Base class for all provider simulators with common estimation logic.
/// </summary>
public abstract class BaseProviderSimulator : IProviderSimulator
{
    protected readonly ILogger _logger;
    protected readonly IPlannedProcessRepository _plannedProcessRepository;
    protected readonly Random _random = new();
    protected readonly Dictionary<ProcessType, double> _standardDurations;
    protected readonly WorkingHoursModel _workingHours;

    public ProviderModel Provider { get; protected set; } = new();

    protected BaseProviderSimulator(
        ILogger logger,
        IPlannedProcessRepository plannedProcessRepository,
        Dictionary<ProcessType, double> standardDurations,
        WorkingHoursModel? workingHours = null)
    {
        _logger = logger;
        _plannedProcessRepository = plannedProcessRepository;
        _standardDurations = standardDurations;
        _workingHours = workingHours ?? new WorkingHoursModel(); // Default working hours if not provided
    }


    public async Task<ProposalModel> HandleProposalAsync(ProposeProcessToProviderCommand proposal)
    {
        var proposalModel = new ProposalModel
        {
            RequestId = proposal.RequestId,
            ProviderId = Provider.Id,
            Process = proposal.Process,
            ArrivedAt = DateTime.UtcNow,
            MotorSpecs = proposal.MotorSpecs
        };

        // Get process capability using normalized process name
        var processCapability = Provider.ProcessCapabilities.FirstOrDefault(pc => pc.Process == proposal.Process);
        
        if (processCapability == null)
        {
            proposalModel.Status = ProposalStatus.Declined;
            proposalModel.DeclineReason = $"{Provider.Name} does not have capability for {proposal.Process}";
            proposalModel.ModifiedAt = DateTime.UtcNow;

            return proposalModel;
        }

        // Generate estimate with duration
        var estimate = await GenerateEstimateAsync(proposal, processCapability);
        
        // Check if we have available time slots
        if (estimate.AvailableTimeSlots.Count == 0)
        {
            proposalModel.Status = ProposalStatus.Declined;
            proposalModel.DeclineReason = $"{Provider.Name} has no available capacity in the requested time window";
            proposalModel.ModifiedAt = DateTime.UtcNow;
            return proposalModel;
        }

        // Accept proposal and set estimate
        proposalModel.Status = ProposalStatus.Accepted;
        proposalModel.Estimate = estimate;
        proposalModel.ModifiedAt = DateTime.UtcNow;

        return proposalModel;
    }

    public void HandleConfirmation(ProposalEntity proposalEntity)
    {
        // In a real implementation, update internal state, schedule resources, etc.
    }
    
    /// <summary>
    /// Split a time window into working segments accounting for breaks, lunch, end of day, etc.
    /// </summary>
    public Task<List<TimeWindowModel>> GetWorkingSegmentsAsync(TimeWindowModel allocatedSlot)
    {
        var segments = SplitIntoWorkingSegments(allocatedSlot);
        return Task.FromResult(segments);
    }
    
    /// <summary>
    /// Get all segments (working time + breaks) for the allocated slot.
    /// This provides complete timeline with segment types.
    /// </summary>
    public Task<List<TimeSegmentModel>> GetAllSegmentsAsync(TimeWindowModel allocatedSlot)
    {
        var allSegments = new List<TimeSegmentModel>();
        var workingSegments = SplitIntoWorkingSegments(allocatedSlot);
        
        if (workingSegments.Count == 0)
            return Task.FromResult(allSegments);
        
        var currentTime = allocatedSlot.StartTime;
        int order = 0;
        
        foreach (var workSegment in workingSegments)
        {
            // Add break segment if there's a gap before this working segment
            if (currentTime < workSegment.StartTime)
            {
                allSegments.Add(new TimeSegmentModel
                {
                    StartTime = currentTime,
                    EndTime = workSegment.StartTime,
                    SegmentOrder = order++,
                    SegmentType = SegmentType.Break
                });
            }
            
            // Add working segment
            allSegments.Add(new TimeSegmentModel
            {
                StartTime = workSegment.StartTime,
                EndTime = workSegment.EndTime,
                SegmentOrder = order++,
                SegmentType = SegmentType.WorkingTime
            });
            
            currentTime = workSegment.EndTime;
        }
        
        // Add final break segment if process doesn't end exactly at last working segment
        if (currentTime < allocatedSlot.EndTime)
        {
            allSegments.Add(new TimeSegmentModel
            {
                StartTime = currentTime,
                EndTime = allocatedSlot.EndTime,
                SegmentOrder = order++,
                SegmentType = SegmentType.Break
            });
        }
        
        return Task.FromResult(allSegments);
    }

    private async Task<ProcessEstimateModel> GenerateEstimateAsync(ProposeProcessToProviderCommand proposal, ProcessCapabilityModel capability)
    {
        // Normalize activity name using ProcessType
        double baseHours;
        if (!_standardDurations.TryGetValue(proposal.Process, out baseHours))
        {
            baseHours = 8.0;
        }

        // Apply provider's speed multiplier and add randomness ±30%
        var timeVariance = baseHours * 0.3;
        var actualHours = (baseHours * capability.SpeedMultiplier) + (_random.NextDouble() * 2 - 1) * timeVariance;

        // Calculate cost: CostPerHour * ActualHours, with ±20% variance
        var baseCost = capability.CostPerHour * (decimal)actualHours;
        var costVariance = baseCost * 0.2m;
        var actualCost = baseCost + (decimal)(_random.NextDouble() * 2 - 1) * costVariance;

        // Calculate emissions: Energy (kWh/h) * Hours * Carbon Intensity (kgCO2/kWh)
        var emissions = capability.EnergyConsumptionKwhPerHour 
            * actualHours 
            * capability.CarbonIntensityKgCO2PerKwh;

        var estimate = new ProcessEstimateModel
        {
            Cost = actualCost,
            QualityScore = capability.QualityScore,
            EmissionsKgCO2 = emissions
        };

        // Calculate available time slots if time window is provided
        if (proposal.RequestedTimeWindow != null)
        {
            estimate.AvailableTimeSlots = await CalculateAvailableTimeSlotsAsync(proposal.RequestedTimeWindow, actualHours);
        }

        return estimate;
    }

    /// <summary>
    /// Calculates available time slots for a process.
    /// Algorithm:
    /// 1. Get all working segments (accounting for breaks, lunch, weekends)
    /// 2. Subtract planned processes from working segments -> free segments
    /// 3. For each hour in the day, try to accumulate required hours from consecutive free segments
    /// 4. Return unique slots (by StartTime/EndTime)
    /// </summary>
    private async Task<List<TimeWindowModel>> CalculateAvailableTimeSlotsAsync(TimeWindowModel requestedWindow, double requiredWorkingHours)
    {
        // Step 1: Get all working segments (respecting breaks, lunch, weekends)
        var workingSegments = SplitIntoWorkingSegments(requestedWindow);
        
        if (workingSegments.Count == 0)
            return new List<TimeWindowModel>();

        // Step 2: Get planned processes and subtract them from working segments
        var plannedProcesses = await _plannedProcessRepository.GetAllInTimeWindowAsync(
            Provider.Id, requestedWindow.StartTime, requestedWindow.EndTime);
        
        var freeSegments = SubtractPlannedProcesses(workingSegments, plannedProcesses);
        
        if (freeSegments.Count == 0)
            return new List<TimeWindowModel>();

        // Step 3: Generate slots with 1-hour step, starting from each hour mark
        var candidateSlots = new List<TimeWindowModel>();
        
        // Find the first working hour
        var currentHour = new DateTime(
            requestedWindow.StartTime.Year,
            requestedWindow.StartTime.Month,
            requestedWindow.StartTime.Day,
            requestedWindow.StartTime.Hour,
            0, 0, DateTimeKind.Utc);
        
        while (currentHour < requestedWindow.EndTime)
        {
            // Check if this hour falls within any free segment
            var slot = TryBuildSlotFromHour(currentHour, requiredWorkingHours, freeSegments);
            if (slot != null)
            {
                candidateSlots.Add(slot);
            }
            
            // Move to next hour
            currentHour = currentHour.AddHours(1);
        }

        // Step 4: Remove duplicates (same start/end times)
        var uniqueSlots = candidateSlots
            .GroupBy(s => new { s.StartTime, s.EndTime })
            .Select(g => g.First())
            .OrderBy(s => s.StartTime)
            .ToList();

        // Step 5: Populate segments for each slot
        foreach (var slot in uniqueSlots)
        {
            slot.Segments = (await GetAllSegmentsAsync(slot)).ToList();
        }

        return uniqueSlots;
    }

    /// <summary>
    /// Subtracts planned processes from working segments to get free time segments.
    /// Only subtracts WorkingTime segments, not breaks.
    /// </summary>
    private List<TimeWindowModel> SubtractPlannedProcesses(
        List<TimeWindowModel> workingSegments,
        List<PlannedProcessEntity> plannedProcesses)
    {
        var freeSegments = new List<TimeWindowModel>();

        foreach (var segment in workingSegments)
        {
            var currentSegments = new List<TimeWindowModel> { segment };

            // For each planned process, subtract only its WorkingTime segments
            foreach (var planned in plannedProcesses)
            {
                // Get only WorkingTime segments from this planned process
                var busySegments = planned.AllocatedSlot.Segments
                    .Where(s => s.SegmentType == "WorkingTime")
                    .OrderBy(s => s.StartTime)
                    .ToList();

                // Subtract each busy segment
                foreach (var busySegment in busySegments)
                {
                    var newSegments = new List<TimeWindowModel>();

                    foreach (var seg in currentSegments)
                    {
                        // Check if busy segment overlaps with this segment
                        if (busySegment.EndTime <= seg.StartTime || busySegment.StartTime >= seg.EndTime)
                        {
                            // No overlap, keep segment as is
                            newSegments.Add(seg);
                        }
                        else
                        {
                            // Overlap - split segment
                            // Add segment before busy period
                            if (seg.StartTime < busySegment.StartTime)
                            {
                                newSegments.Add(new TimeWindowModel
                                {
                                    StartTime = seg.StartTime,
                                    EndTime = busySegment.StartTime
                                });
                            }

                            // Add segment after busy period
                            if (seg.EndTime > busySegment.EndTime)
                            {
                                newSegments.Add(new TimeWindowModel
                                {
                                    StartTime = busySegment.EndTime,
                                    EndTime = seg.EndTime
                                });
                            }
                        }
                    }

                    currentSegments = newSegments;
                }
            }

            freeSegments.AddRange(currentSegments);
        }

        return freeSegments.OrderBy(s => s.StartTime).ToList();
    }

    /// <summary>
    /// Tries to build a slot starting from the specified hour that accumulates required working hours.
    /// Takes only the necessary time from each segment.
    /// Returns the slot (StartTime to EndTime) or null if cannot fit.
    /// </summary>
    private TimeWindowModel? TryBuildSlotFromHour(
        DateTime startHour,
        double requiredWorkingHours,
        List<TimeWindowModel> freeSegments)
    {
        double accumulatedHours = 0;
        DateTime? slotStart = null;
        DateTime? slotEnd = null;

        foreach (var segment in freeSegments)
        {
            // Skip segments that end before or at our start hour
            if (segment.EndTime <= startHour)
                continue;
            
            // Skip segments that start before our start hour
            if (segment.StartTime < startHour)
            {
                // Only use the part of segment after startHour
                var effectiveStart = startHour;
                
                // Skip if startHour is at or after segment end
                if (effectiveStart >= segment.EndTime)
                    continue;
                
                // Set slot start if this is the first contributing segment
                if (!slotStart.HasValue)
                {
                    slotStart = effectiveStart;
                }

                // Calculate how many hours are available from startHour to segment end
                var availableHours = (segment.EndTime - effectiveStart).TotalHours;
                
                // Calculate how many hours we still need
                var hoursNeeded = requiredWorkingHours - accumulatedHours;
                
                if (availableHours >= hoursNeeded)
                {
                    // This segment has enough hours to complete the slot
                    slotEnd = effectiveStart.AddHours(hoursNeeded);
                    return new TimeWindowModel
                    {
                        StartTime = slotStart.Value,
                        EndTime = slotEnd.Value
                    };
                }
                else
                {
                    // Take all available hours from this segment and continue
                    accumulatedHours += availableHours;
                    slotEnd = segment.EndTime;
                }
            }
            else
            {
                // Segment starts at or after startHour - use it fully
                var effectiveStart = segment.StartTime;
                
                // Set slot start if this is the first contributing segment
                if (!slotStart.HasValue)
                {
                    slotStart = effectiveStart;
                }

                // Calculate how many hours are available in this segment
                var availableHours = (segment.EndTime - effectiveStart).TotalHours;
                
                // Calculate how many hours we still need
                var hoursNeeded = requiredWorkingHours - accumulatedHours;
                
                if (availableHours >= hoursNeeded)
                {
                    // This segment has enough hours to complete the slot
                    slotEnd = effectiveStart.AddHours(hoursNeeded);
                    return new TimeWindowModel
                    {
                        StartTime = slotStart.Value,
                        EndTime = slotEnd.Value
                    };
                }
                else
                {
                    // Take all available hours from this segment and continue
                    accumulatedHours += availableHours;
                    slotEnd = segment.EndTime;
                }
            }
        }

        // Could not accumulate enough hours
        return null;
    }

    /// <summary>
    /// Splits a time window into segments that respect working hours, breaks, and weekends.
    /// Each segment represents a continuous period of work without interruptions.
    /// </summary>
    private List<TimeWindowModel> SplitIntoWorkingSegments(TimeWindowModel window)
    {
        var segments = new List<TimeWindowModel>();
        
        if (_workingHours.Is24x7)
        {
            segments.Add(window);
            return segments;
        }
        
        var current = _workingHours.GetNextWorkingTime(window.StartTime);
        
        while (current < window.EndTime)
        {
            var dayEnd = _workingHours.GetWorkDayEnd(current);
            var segmentEnd = window.EndTime < dayEnd ? window.EndTime : dayEnd;
            
            // Collect all breaks for this day (lunch + additional)
            var breaksForDay = new List<(DateTime Start, DateTime End, string Name)>();
            
            // Add lunch break
            var lunchStart = new DateTime(current.Year, current.Month, current.Day, 
                _workingHours.LunchBreakStartHour, 0, 0);
            var lunchEnd = lunchStart.AddMinutes(_workingHours.LunchBreakDurationMinutes);
            breaksForDay.Add((lunchStart, lunchEnd, "Lunch"));
            
            // Add additional breaks
            foreach (var breakPeriod in _workingHours.AdditionalBreaks)
            {
                var breakStart = breakPeriod.GetStartTime(current.Date);
                var breakEnd = breakPeriod.GetEndTime(current.Date);
                
                // Only include breaks that fall within the working day
                if (breakStart >= current && breakEnd <= segmentEnd)
                {
                    breaksForDay.Add((breakStart, breakEnd, breakPeriod.Name));
                }
            }
            
            // Sort breaks by start time
            breaksForDay = breaksForDay.OrderBy(b => b.Start).ToList();
            
            // Split the day segment around all breaks
            var dayCurrent = current;
            
            foreach (var breakInfo in breaksForDay)
            {
                if (dayCurrent < breakInfo.Start && segmentEnd > breakInfo.Start)
                {
                    // Add segment before break
                    segments.Add(new TimeWindowModel
                    {
                        StartTime = dayCurrent,
                        EndTime = breakInfo.Start
                    });
                    
                    // Move to after break
                    dayCurrent = breakInfo.End;
                }
                else if (dayCurrent >= breakInfo.Start && dayCurrent < breakInfo.End)
                {
                    // Current time is during break, skip to after break
                    dayCurrent = breakInfo.End;
                }
            }
            
            // Add remaining segment after all breaks (if any)
            if (dayCurrent < segmentEnd)
            {
                segments.Add(new TimeWindowModel
                {
                    StartTime = dayCurrent,
                    EndTime = segmentEnd
                });
            }
            
            // Move to next work day
            current = _workingHours.GetNextWorkingTime(dayEnd.AddMinutes(1));
        }
        
        
        return segments;
    }
}
