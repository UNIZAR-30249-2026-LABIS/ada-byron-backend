namespace AdaByron.Application.DTOs;

public sealed class CreateReservationRequest
{
    public string RequesterEmail { get; set; } = string.Empty;
    public string SpaceId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int AttendeeCount { get; set; }
}

public sealed class ReservationDto
{
    public Guid Id { get; set; }
    public string RequesterEmail { get; set; } = string.Empty;
    public string SpaceName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class IdentifyByEmailDto
{
    public string Email { get; set; } = string.Empty;
}
